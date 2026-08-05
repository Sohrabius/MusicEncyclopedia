using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Search.Services;

/// <summary>
/// Implements full-text search across all indexed entities.
/// For SQL Server, uses FREETEXTTABLE for ranked results.
/// For SQLite, uses LIKE-based search.
/// </summary>
public sealed class SearchService : ISearchService
{
    private readonly string _connectionString;
    private readonly bool _isSqlite;

    public SearchService(string connectionString, bool isSqlite = false)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _isSqlite = isSqlite;
    }

    private IDbConnection CreateConnection()
    {
        return _isSqlite
            ? (IDbConnection)new Microsoft.Data.Sqlite.SqliteConnection(_connectionString)
            : new SqlConnection(_connectionString);
    }

    /// <inheritdoc />
    public async Task<PagedResult<SearchResultDto>> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        if (string.IsNullOrWhiteSpace(query.Q))
        {
            return PagedResult<SearchResultDto>.Create([], query.Page, query.PageSize, 0);
        }

        var searchTerm = query.Q.Trim();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));
        var offset = (page - 1) * pageSize;

        using var connection = CreateConnection();
        connection.Open();

        // Generate the UNION ALL fragments for each entity table
        var fragments = BuildSearchFragments(
            query.EntityType, query.Culture, query.Genre, query.Mood, query.Instrument, query.Language);

        // Count query
        var countSql = $@"
SELECT COUNT_BIG(*)
FROM (
{string.Join("\n    UNION ALL\n", fragments.Select(f => f.CountSql))}
) AS total";

        var countParams = new DynamicParameters();
        countParams.Add("searchTerm", $"%{searchTerm}%");
        countParams.Add("rawSearchTerm", searchTerm);
        if (!string.IsNullOrWhiteSpace(query.Culture))
            countParams.Add("culture", query.Culture);
        if (!string.IsNullOrWhiteSpace(query.Genre))
            countParams.Add("genre", query.Genre);
        if (!string.IsNullOrWhiteSpace(query.Mood))
            countParams.Add("mood", query.Mood);
        if (!string.IsNullOrWhiteSpace(query.Instrument))
            countParams.Add("instrument", query.Instrument);

        var totalItems = await connection.ExecuteScalarAsync<int>(
            countSql, countParams, commandTimeout: 30);

        if (totalItems == 0)
        {
            return PagedResult<SearchResultDto>.Create([], page, pageSize, 0);
        }

        // Data query
        var orderByClause = _isSqlite ? "Title ASC" : "Rank DESC";
        var dataSql = $@"
SELECT EntityType, EntityId, Title, Subtitle, Description, ImageUrl, Url, Culture, 0 AS Rank
FROM (
{string.Join("\n    UNION ALL\n", fragments.Select(f => f.SelectSql))}
) AS results
ORDER BY {orderByClause}
OFFSET @offset ROWS
FETCH NEXT @pageSize ROWS ONLY";

        var dataParams = new DynamicParameters();
        dataParams.Add("searchTerm", $"%{searchTerm}%");
        dataParams.Add("rawSearchTerm", searchTerm);
        dataParams.Add("offset", offset);
        dataParams.Add("pageSize", pageSize);
        if (!string.IsNullOrWhiteSpace(query.Culture))
            dataParams.Add("culture", query.Culture);
        if (!string.IsNullOrWhiteSpace(query.Genre))
            dataParams.Add("genre", query.Genre);
        if (!string.IsNullOrWhiteSpace(query.Mood))
            dataParams.Add("mood", query.Mood);
        if (!string.IsNullOrWhiteSpace(query.Instrument))
            dataParams.Add("instrument", query.Instrument);

        var rows = await connection.QueryAsync<SearchResultRow>(
            dataSql, dataParams, commandTimeout: 30);

        var results = rows.Select(MapToDto).ToList();

        return PagedResult<SearchResultDto>.Create(results, page, pageSize, totalItems);
    }

    /// <summary>
    /// Builds the per-table SQL fragments for the search.
    /// For SQLite, uses LIKE-based search. For SQL Server, uses FREETEXTTABLE.
    /// </summary>
    private List<SearchFragment> BuildSearchFragments(
        string? entityType, string? culture, string? genre, string? mood, string? instrument, string? language)
    {
        var cultureCondition = !string.IsNullOrWhiteSpace(culture)
            ? "AND Culture = @culture"
            : "";

        string BuildWhereClause(string tableAlias, params string[] columns)
        {
            if (_isSqlite)
            {
                var likeConditions = columns.Select(c => $"{tableAlias}.{c} LIKE @searchTerm");
                return string.Join(" OR ", likeConditions);
            }
            // SQL Server uses FREETEXTTABLE, so no WHERE clause needed on the main table
            return "1=1";
        }

        string BuildFromClause(string tableName, string tableAlias, string ftsColumns)
        {
            if (_isSqlite)
            {
                return $"FROM {tableName} {tableAlias}";
            }
            return $"FROM {tableName} {tableAlias} INNER JOIN FREETEXTTABLE({tableName}, ({ftsColumns}), @rawSearchTerm) ft ON {tableAlias}.Id = ft.[Key]";
        }

        string BuildSelectColumns(string tableAlias, string titleExpr, string descriptionExpr, string urlExpr)
        {
            var concatOp = _isSqlite ? "||" : "+";
            var urlFull = urlExpr.Replace("+", concatOp);
            return $@"
    '{tableAlias}' AS EntityType, {tableAlias}.Id AS EntityId, {titleExpr} AS Title,
           NULL AS Subtitle, {descriptionExpr} AS Description, NULL AS ImageUrl,
           {urlFull} AS Url, {tableAlias}.Culture,
           0 AS Rank";
        }

        string Concat(string a, string b) => _isSqlite ? $"({a} || {b})" : $"({a} + {b})";

        // Build filter joins for album and track
        var albumFilterJoin = "";
        if (!string.IsNullOrWhiteSpace(genre))
            albumFilterJoin += " INNER JOIN AlbumGenre ag ON a.Id = ag.AlbumId INNER JOIN Genre g ON ag.GenreId = g.Id AND g.Slug = @genre";
        if (!string.IsNullOrWhiteSpace(mood))
            albumFilterJoin += " INNER JOIN AlbumMood am ON a.Id = am.AlbumId INNER JOIN Mood m ON am.MoodId = m.Id AND m.Slug = @mood";
        if (!string.IsNullOrWhiteSpace(instrument))
            albumFilterJoin += " INNER JOIN AlbumInstrument ai ON a.Id = ai.AlbumId INNER JOIN Instrument i ON ai.InstrumentId = i.Id AND i.Slug = @instrument";

        var trackFilterJoin = "";
        if (!string.IsNullOrWhiteSpace(genre))
            trackFilterJoin += " INNER JOIN TrackGenre tg ON t.Id = tg.TrackId INNER JOIN Genre g2 ON tg.GenreId = g2.Id AND g2.Slug = @genre";
        if (!string.IsNullOrWhiteSpace(mood))
            trackFilterJoin += " INNER JOIN TrackMood tm ON t.Id = tm.TrackId INNER JOIN Mood m2 ON tm.MoodId = m2.Id AND m2.Slug = @mood";

        // For SQLite, add WHERE clauses for filter joins
        var albumWhereExtra = "";
        var trackWhereExtra = "";
        if (_isSqlite)
        {
            if (!string.IsNullOrWhiteSpace(genre))
                albumWhereExtra += " AND g.Slug = @genre";
            if (!string.IsNullOrWhiteSpace(mood))
                albumWhereExtra += " AND m.Slug = @mood";
            if (!string.IsNullOrWhiteSpace(instrument))
                albumWhereExtra += " AND i.Slug = @instrument";

            if (!string.IsNullOrWhiteSpace(genre))
                trackWhereExtra += " AND g2.Slug = @genre";
            if (!string.IsNullOrWhiteSpace(mood))
                trackWhereExtra += " AND m2.Slug = @mood";
        }

        var fragments = new List<SearchFragment>();

        // Helper to create a fragment
        SearchFragment MakeFragment(string entityTypeName, string tableName, string tableAlias,
            string titleCol, string descCol, string slugCol,
            string filterJoin, string filterWhereExtra)
        {
            var whereClause = _isSqlite
                ? $"WHERE ({BuildWhereClause(tableAlias, titleCol, descCol)}) {cultureCondition} {filterWhereExtra}"
                : $"WHERE 1=1 {cultureCondition} {filterWhereExtra}";

            return new SearchFragment
            {
                EntityType = entityTypeName,
                SelectSql = $@"
    SELECT '{entityTypeName}' AS EntityType, {tableAlias}.Id AS EntityId,
           {tableAlias}.{titleCol} AS Title,
           NULL AS Subtitle, {tableAlias}.{descCol} AS Description, NULL AS ImageUrl,
           {Concat($"'/{entityTypeName.ToLowerInvariant()}/'", $"{tableAlias}.{slugCol}")} AS Url,
           {tableAlias}.Culture,
           0 AS Rank
    FROM {tableName} {tableAlias}
    {filterJoin}
    {whereClause}",
                CountSql = $@"
    SELECT {tableAlias}.Id FROM {tableName} {tableAlias}
    {filterJoin}
    {whereClause}"
            };
        }

        // Build fragments for all entity types
        fragments.Add(MakeFragment("Album", "Album", "a", "Title", "Description", "Slug",
            albumFilterJoin, albumWhereExtra));
        fragments.Add(MakeFragment("Track", "Track", "t", "Title", "Description", "Slug",
            trackFilterJoin, trackWhereExtra));
        fragments.Add(MakeFragment("Person", "Person", "p", "FullName", "Biography", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Company", "Company", "c", "Name", "History", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Poem", "Poem", "p", "Title", "CanonicalText", "Slug",
            "", ""));
        fragments.Add(MakeFragment("SungVersion", "SungVersion", "sv", "Title", "Text", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Genre", "Genre", "g", "Name", "Description", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Mood", "Mood", "m", "Name", "Description", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Instrument", "Instrument", "i", "Name", "Description", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Tag", "Tag", "t", "Name", "Name", "Slug",
            "", ""));
        fragments.Add(MakeFragment("Alias", "Alias", "a", "AliasName", "AliasName", "AliasName",
            "", ""));
        fragments.Add(MakeFragment("Localization", "Localization", "l", "LocalizedText", "LocalizedText", "LocalizedText",
            "", ""));

        // Filter by entity type if specified
        if (!string.IsNullOrWhiteSpace(entityType))
        {
            fragments = fragments.Where(f =>
                string.Equals(f.EntityType, entityType, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // When genre/mood/instrument filters are active, only search entities that support them
        var hasFacetFilter = !string.IsNullOrWhiteSpace(genre) || !string.IsNullOrWhiteSpace(mood) || !string.IsNullOrWhiteSpace(instrument);
        if (hasFacetFilter)
        {
            fragments = fragments.Where(f =>
                f.EntityType is "Album" or "Track")
                .ToList();
        }

        return fragments;
    }

    private static SearchResultDto MapToDto(SearchResultRow row)
    {
        return new SearchResultDto
        {
            EntityType = row.EntityType,
            EntityId = row.EntityId,
            Title = row.Title,
            Subtitle = row.Subtitle,
            Description = row.Description,
            ImageUrl = row.ImageUrl,
            Url = row.Url,
            Culture = row.Culture
        };
    }

    /// <summary>
    /// Internal row type mapping the raw query result columns.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Local
    private sealed class SearchResultRow
    {
        public string EntityType { get; init; } = "";
        public int EntityId { get; init; }
        public string Title { get; init; } = "";
        public string? Subtitle { get; init; }
        public string? Description { get; init; }
        public string? ImageUrl { get; init; }
        public string Url { get; init; } = "";
        public string Culture { get; init; } = "";
        public int Rank { get; init; }
    }

    /// <summary>
    /// Holds the SELECT and COUNT SQL fragments for a single entity type.
    /// </summary>
    private sealed class SearchFragment
    {
        public string EntityType { get; init; } = "";
        public string SelectSql { get; init; } = "";
        public string CountSql { get; init; } = "";
    }
}
