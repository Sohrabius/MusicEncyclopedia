using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Search.Services;

/// <summary>
/// Implements full-text search across all indexed entities.
/// For SQL Server, uses FREETEXTTABLE for ranked results when the database has
/// full-text indexes (see FullTextSearch.sql); otherwise falls back to LIKE-based
/// search so a SQL Server instance without the Full-Text Search feature (or
/// without the indexes applied) still returns results.
/// </summary>
public sealed class SearchService : ISearchService
{
    private readonly string _connectionString;
    private readonly ICacheService? _cache;

    // Process-wide memo of whether the SQL Server database has full-text
    // indexes. Probed lazily on the first search and cached; a probe failure
    // (e.g. Full-Text Search component not installed) resolves to "not available".
    private static bool _fullTextProbeAttempted;
    private static bool _fullTextAvailable;

    public SearchService(string connectionString, ICacheService? cache = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _cache = cache;
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
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

        // Spec §15.1 search cache key: search:{culture}:{hash}. Only successful
        // results are cached; failures propagate to the caller.
        if (_cache is not null)
        {
            var cacheKey = CacheKeys.Search(
                query.Culture ?? "", query.Q, query.EntityType, query.Genre, query.Mood,
                query.Instrument, query.Page, query.PageSize);

            var cached = await _cache.GetAsync<PagedResult<SearchResultDto>>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await SearchCoreAsync(query, cancellationToken);
            await _cache.SetAsync(cacheKey, result, CacheKeys.SearchDuration, cancellationToken);
            return result;
        }

        return await SearchCoreAsync(query, cancellationToken);
    }

    /// <summary>
    /// Determines whether the SQL Server database can answer FREETEXTTABLE
    /// queries. The result is memoized process-wide after the first probe
    /// (an empty result set is itself a failure signal).
    /// </summary>
    private async Task<bool> CanUseFullTextAsync(
        IDbConnection connection, CancellationToken cancellationToken)
    {
        if (_fullTextProbeAttempted)
        {
            return _fullTextAvailable;
        }

        try
        {
            const string probeSql = """
                SELECT COUNT(*) FROM sys.fulltext_indexes
                WHERE object_id IN (
                    OBJECT_ID(N'Album'), OBJECT_ID(N'Track'), OBJECT_ID(N'Person'),
                    OBJECT_ID(N'Company'), OBJECT_ID(N'Poem'), OBJECT_ID(N'SungVersion'),
                    OBJECT_ID(N'Genre'), OBJECT_ID(N'Mood'), OBJECT_ID(N'Instrument'),
                    OBJECT_ID(N'Source'), OBJECT_ID(N'Location'), OBJECT_ID(N'Publication'),
                    OBJECT_ID(N'RecordingSession'), OBJECT_ID(N'PerformanceEvent'))
                """;
            var indexCount = await connection.ExecuteScalarAsync<int>(
                probeSql, commandTimeout: 10);
            _fullTextAvailable = indexCount > 0;
        }
        catch
        {
            // Full-Text Search component not installed (or permission denied) —
            // fall back to LIKE for the lifetime of this process.
            _fullTextAvailable = false;
        }
        finally
        {
            _fullTextProbeAttempted = true;
        }

        return _fullTextAvailable;
    }

    private async Task<PagedResult<SearchResultDto>> SearchCoreAsync(
        SearchQuery query,
        CancellationToken cancellationToken)
    {
        // SearchAsync guards against a null/empty Q before calling this method.
        var searchTerm = query.Q!.Trim();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Max(1, Math.Min(100, query.PageSize));
        var offset = (page - 1) * pageSize;

        using var connection = CreateConnection();
        connection.Open();

        // Generate the UNION ALL fragments for each entity table. SQL Server only
        // uses FREETEXTTABLE when the database actually has full-text indexes;
        // otherwise the LIKE path is used.
        var useFullText = await CanUseFullTextAsync(connection, cancellationToken);
        var fragments = BuildSearchFragments(
            query.EntityType, query.Genre, query.Mood, query.Instrument, useFullText);

        // An unsupported entity-type filter produces no SQL fragments. Return a
        // valid empty page instead of generating an invalid FROM () query. API
        // callers reject this input earlier; this guard also protects MVC and
        // direct service callers.
        if (fragments.Count == 0)
        {
            return PagedResult<SearchResultDto>.Create([], page, pageSize, 0);
        }

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

        // Data query. Ranking (Rank DESC) is only available on the full-text path;
        // the LIKE path orders by title.
        var orderByClause = useFullText ? "Rank DESC" : "Title ASC";
        var pagination = "OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

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
        string? entityType, string? genre, string? mood, string? instrument,
        bool useFullText)
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

        // Helper to create a fragment. titleExpr/descExpr are the SELECT and LIKE
        // search expressions; ftsColumns is the comma-separated list of REAL indexed
        // columns for the SQL Server FREETEXTTABLE join (must match FullTextSearch.sql).
        void MakeFragment(string entityTypeName, string table, string tableAlias,
            string pkColumn, string titleExpr, string descExpr, string ftsColumns,
            string slugColumn, string filterJoin)
        {
            string fromClause;
            string rankExpr;
            string whereClause;

            if (!useFullText)
            {
                fromClause = $"FROM {table} {tableAlias}";
                rankExpr = "0 AS Rank";
                whereClause = $"WHERE ({titleExpr} LIKE @searchTerm OR {descExpr} LIKE @searchTerm) AND {tableAlias}.IsDeleted = 0";
            }
            else
            {
                fromClause = $"FROM {table} {tableAlias} INNER JOIN FREETEXTTABLE({table}, ({ftsColumns}), @rawSearchTerm) ft ON {tableAlias}.{pkColumn} = ft.[Key]";
                rankExpr = "ft.Rank AS Rank";
                whereClause = $"WHERE {tableAlias}.IsDeleted = 0";
            }

            fragments.Add(new SearchFragment
            {
                EntityType = entityTypeName,
                SelectSql = $@"
    SELECT '{entityTypeName}' AS EntityType, {tableAlias}.{pkColumn} AS EntityId,
           {titleExpr} AS Title,
           NULL AS Subtitle, {descExpr} AS Description, NULL AS ImageUrl,
           {tableAlias}.{slugColumn} AS UrlSlug,
           {rankExpr}
    {fromClause}
    {filterJoin}
    {whereClause}",
                CountSql = $@"
    SELECT {tableAlias}.{pkColumn}
    {fromClause}
    {filterJoin}
    {whereClause}"
            });
        }

        // All entity types that have public pages (Tag, Alias and Localization are
        // cross-cutting tables without searchable detail pages and are excluded).
        // The ftsColumns list must match the full-text indexes in FullTextSearch.sql.
        MakeFragment("Album", "Album", "a", "AlbumId",
            "a.Title", "a.Description", "Title, Description, OriginalTitle, EnglishTitle", "Slug", albumFilterJoin);
        MakeFragment("Track", "Track", "t", "TrackId",
            "t.Title", "t.Description", "Title, Description, OriginalTitle, EnglishTitle", "Slug", trackFilterJoin);
        MakeFragment("Person", "Person", "p", "PersonId",
            "p.FullName", "p.Biography", "FullName, Biography, OriginalName, EnglishName", "Slug", "");
        MakeFragment("Company", "Company", "c", "CompanyId",
            "c.Name", "c.History", "Name, History, OriginalName, EnglishName", "Slug", "");
        MakeFragment("Poem", "Poem", "po", "PoemId",
            "po.Title", "po.CanonicalText", "Title, CanonicalText, OriginalTitle, EnglishTitle", "Slug", "");
        MakeFragment("SungVersion", "SungVersion", "sv", "SungVersionId",
            "sv.Title", "sv.Text", "Title, Text", "Slug", "");
        MakeFragment("Genre", "Genre", "g", "GenreId",
            "g.Name", "g.Description", "Name, Description", "Slug", "");
        MakeFragment("Mood", "Mood", "m", "MoodId",
            "m.Name", "m.Description", "Name, Description", "Slug", "");
        MakeFragment("Instrument", "Instrument", "i", "InstrumentId",
            "i.Name", "i.Description", "Name, Description", "Slug", "");

        // Source and Location have no Description column; their Author/Name still match.
        MakeFragment("Source", "Source", "s", "SourceId",
            "s.Title", "s.Author", "Title, Author", "Slug", "");
        MakeFragment("Location", "Location", "l", "LocationId",
            "l.Name", "NULL", "Name", "Slug", "");
        MakeFragment("Publication", "Publication", "pu", "PublicationId",
            "pu.Title", "NULL", "Title", "Slug", "");

        // Recording sessions and performance events have no Title column; the site
        // titles them by their type name, so search results do the same.
        MakeFragment("RecordingSession", "RecordingSession", "rs", "RecordingSessionId",
            "(CASE WHEN st.Name IS NULL THEN 'Recording Session' ELSE st.Name END)",
            "rs.Notes", "Notes", "Slug",
            "LEFT JOIN SessionType st ON st.SessionTypeId = rs.SessionTypeId");
        MakeFragment("PerformanceEvent", "PerformanceEvent", "pe", "PerformanceEventId",
            "(CASE WHEN et.Name IS NULL THEN 'Performance Event' ELSE et.Name END)",
            "pe.PerformanceNotes", "PerformanceNotes", "Slug",
            "LEFT JOIN EventType et ON et.EventTypeId = pe.EventTypeId");

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
        "Publication" => "publications",
        "RecordingSession" => "sessions",
        "PerformanceEvent" => "events",
        "Location" => "locations",
        "Source" => "sources",
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
