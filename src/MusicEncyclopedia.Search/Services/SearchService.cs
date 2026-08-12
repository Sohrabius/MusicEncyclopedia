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
            query.EntityType, query.Genre, query.Mood, query.Instrument);

        // Count query
        var countSql = $@"
SELECT COUNT(*)
FROM (
{string.Join("\n    UNION ALL\n", fragments.Select(f => f.CountSql))}
) AS total";

        var countParams = new DynamicParameters();
        countParams.Add("searchTerm", $"%{searchTerm}%");
        countParams.Add("rawSearchTerm", searchTerm);
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
        var pagination = _isSqlite
            ? "LIMIT @pageSize OFFSET @offset"
            : "OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

        var dataSql = $@"
SELECT EntityType, EntityId, Title, Subtitle, Description, ImageUrl, UrlSlug, Rank
FROM (
{string.Join("\n    UNION ALL\n", fragments.Select(f => f.SelectSql))}
) AS results
ORDER BY {orderByClause}
{pagination}";

        var dataParams = new DynamicParameters();
        dataParams.Add("searchTerm", $"%{searchTerm}%");
        dataParams.Add("rawSearchTerm", searchTerm);
        dataParams.Add("offset", offset);
        dataParams.Add("pageSize", pageSize);
        if (!string.IsNullOrWhiteSpace(query.Genre))
            dataParams.Add("genre", query.Genre);
        if (!string.IsNullOrWhiteSpace(query.Mood))
            dataParams.Add("mood", query.Mood);
        if (!string.IsNullOrWhiteSpace(query.Instrument))
            dataParams.Add("instrument", query.Instrument);

        var rows = await connection.QueryAsync<SearchResultRow>(
            dataSql, dataParams, commandTimeout: 30);

        var results = rows.Select(r => MapToDto(r, query.Culture ?? "")).ToList();

        return PagedResult<SearchResultDto>.Create(results, page, pageSize, totalItems);
    }

    /// <summary>
    /// Builds the per-table SQL fragments for the search.
    /// Column names and join tables match the EF Core schema exactly.
    /// </summary>
    private List<SearchFragment> BuildSearchFragments(
        string? entityType, string? genre, string? mood, string? instrument)
    {
        // Facet joins for Album (genres and moods only — instruments attach to tracks)
        var albumFilterJoin = "";
        if (!string.IsNullOrWhiteSpace(genre))
            albumFilterJoin += " INNER JOIN AlbumGenre ag ON a.AlbumId = ag.AlbumId INNER JOIN Genre g ON ag.GenreId = g.GenreId AND g.Slug = @genre";
        if (!string.IsNullOrWhiteSpace(mood))
            albumFilterJoin += " INNER JOIN AlbumMood am ON a.AlbumId = am.AlbumId INNER JOIN Mood m ON am.MoodId = m.MoodId AND m.Slug = @mood";

        // Facet joins for Track
        var trackFilterJoin = "";
        if (!string.IsNullOrWhiteSpace(genre))
            trackFilterJoin += " INNER JOIN TrackGenre tg ON t.TrackId = tg.TrackId INNER JOIN Genre g2 ON tg.GenreId = g2.GenreId AND g2.Slug = @genre";
        if (!string.IsNullOrWhiteSpace(mood))
            trackFilterJoin += " INNER JOIN TrackMood tm ON t.TrackId = tm.TrackId INNER JOIN Mood m2 ON tm.MoodId = m2.MoodId AND m2.Slug = @mood";
        if (!string.IsNullOrWhiteSpace(instrument))
            trackFilterJoin += " INNER JOIN TrackInstrument ti ON t.TrackId = ti.TrackId INNER JOIN Instrument i ON ti.InstrumentId = i.InstrumentId AND i.Slug = @instrument";

        var fragments = new List<SearchFragment>();

        // Helper to create a fragment
        void MakeFragment(string entityTypeName, string table, string tableAlias,
            string pkColumn, string titleColumn, string descriptionColumn, string slugColumn,
            string filterJoin)
        {
            string fromClause;
            string rankExpr;
            string whereClause;

            if (_isSqlite)
            {
                fromClause = $"FROM {table} {tableAlias}";
                rankExpr = "0 AS Rank";
                whereClause = $"WHERE ({tableAlias}.{titleColumn} LIKE @searchTerm OR {tableAlias}.{descriptionColumn} LIKE @searchTerm) AND {tableAlias}.IsDeleted = 0";
            }
            else
            {
                fromClause = $"FROM {table} {tableAlias} INNER JOIN FREETEXTTABLE({table}, ({titleColumn}, {descriptionColumn}), @rawSearchTerm) ft ON {tableAlias}.{pkColumn} = ft.[Key]";
                rankExpr = "ft.Rank AS Rank";
                whereClause = $"WHERE {tableAlias}.IsDeleted = 0";
            }

            fragments.Add(new SearchFragment
            {
                EntityType = entityTypeName,
                SelectSql = $@"
    SELECT '{entityTypeName}' AS EntityType, {tableAlias}.{pkColumn} AS EntityId,
           {tableAlias}.{titleColumn} AS Title,
           NULL AS Subtitle, {tableAlias}.{descriptionColumn} AS Description, NULL AS ImageUrl,
           {tableAlias}.{slugColumn} AS UrlSlug,
           {rankExpr}
    {fromClause}
    {filterJoin}
    {whereClause}",
                CountSql = $@"
    SELECT {tableAlias}.{pkColumn} FROM {table} {tableAlias}
    {filterJoin}
    {whereClause}"
            });
        }

        // All entity types that have public pages (Tag, Alias and Localization are
        // cross-cutting tables without searchable detail pages and are excluded).
        MakeFragment("Album", "Album", "a", "AlbumId", "Title", "Description", "Slug", albumFilterJoin);
        MakeFragment("Track", "Track", "t", "TrackId", "Title", "Description", "Slug", trackFilterJoin);
        MakeFragment("Person", "Person", "p", "PersonId", "FullName", "Biography", "Slug", "");
        MakeFragment("Company", "Company", "c", "CompanyId", "Name", "History", "Slug", "");
        MakeFragment("Poem", "Poem", "po", "PoemId", "Title", "CanonicalText", "Slug", "");
        MakeFragment("SungVersion", "SungVersion", "sv", "SungVersionId", "Title", "Text", "Slug", "");
        MakeFragment("Genre", "Genre", "g", "GenreId", "Name", "Description", "Slug", "");
        MakeFragment("Mood", "Mood", "m", "MoodId", "Name", "Description", "Slug", "");
        MakeFragment("Instrument", "Instrument", "i", "InstrumentId", "Name", "Description", "Slug", "");

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

    private static SearchResultDto MapToDto(SearchResultRow row, string culture)
    {
        var routeSegment = GetRouteSegment(row.EntityType);

        return new SearchResultDto
        {
            EntityType = row.EntityType,
            EntityId = row.EntityId,
            Title = row.Title,
            Subtitle = row.Subtitle,
            Description = row.Description,
            ImageUrl = row.ImageUrl,
            Url = string.IsNullOrWhiteSpace(routeSegment)
                ? ""
                : $"/{routeSegment}/{row.UrlSlug}",
            Culture = culture
        };
    }

    /// <summary>
    /// Maps an entity type to its public route segment (relative to the culture prefix).
    /// </summary>
    private static string GetRouteSegment(string entityType) => entityType switch
    {
        "Album" => "albums",
        "Track" => "tracks",
        "Person" => "people",
        "Company" => "companies",
        "Poem" => "poems",
        "SungVersion" => "sung-versions",
        "Genre" => "genres",
        "Mood" => "moods",
        "Instrument" => "instruments",
        _ => ""
    };

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
        public string UrlSlug { get; init; } = "";
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
