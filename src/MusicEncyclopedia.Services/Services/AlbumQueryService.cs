using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Services.Infrastructure;

namespace MusicEncyclopedia.Services.Services;

public sealed class AlbumQueryService : IAlbumQueryService
{
    private readonly string _connectionString;
    private readonly ILogger<AlbumQueryService> _logger;
    private readonly IContentLocalizationService _localizationService;
    private readonly ICacheService _cache;

    public AlbumQueryService(
        IConfiguration configuration,
        ILogger<AlbumQueryService> logger,
        IContentLocalizationService localizationService,
        ICacheService cache)
    {
        _localizationService = localizationService;
        _cache = cache;

        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    public async Task<PagedResult<AlbumListItemDto>> GetAlbumsAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? category = null,
        string? genre = null,
        string? mood = null,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var cacheKey = CacheKeys.List("album", culture, page, pageSize, sort, category, genre, mood, q);

            var cached = await _cache.GetAsync<PagedResult<AlbumListItemDto>>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadAlbumsCoreAsync(
                culture, page, pageSize, sort, category, genre, mood, q, cancellationToken);

            await _cache.SetAsync(cacheKey, result, CacheKeys.ListDuration, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load album list for culture={Culture}, page={Page}", culture, page);
            return PagedResult<AlbumListItemDto>.Create([], page, pageSize, 0);
        }
    }

    private async Task<PagedResult<AlbumListItemDto>> LoadAlbumsCoreAsync(
        string culture,
        int page,
        int pageSize,
        string? sort,
        string? category,
        string? genre,
        string? mood,
        string? q,
        CancellationToken cancellationToken)
    {
        var whereClauses = new List<string> { "a.IsDeleted = 0" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(category))
        {
            whereClauses.Add("ac.Code = @Category");
            parameters.Add("Category", category);
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM AlbumGenre ag_inner JOIN Genre g_inner ON g_inner.GenreId = ag_inner.GenreId WHERE ag_inner.AlbumId = a.AlbumId AND g_inner.Slug = @Genre)");
            parameters.Add("Genre", genre);
        }

        if (!string.IsNullOrWhiteSpace(mood))
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM AlbumMood am_inner JOIN Mood m_inner ON m_inner.MoodId = am_inner.MoodId WHERE am_inner.AlbumId = a.AlbumId AND m_inner.Slug = @Mood)");
            parameters.Add("Mood", mood);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            whereClauses.Add("(a.Title LIKE @Q OR a.OriginalTitle LIKE @Q OR a.EnglishTitle LIKE @Q)");
            parameters.Add("Q", $"%{q}%");
        }

        var whereSql = string.Join(" AND ", whereClauses);

        var orderBy = sort?.ToLowerInvariant() switch
        {
            "title" => "a.Title ASC",
            "createdat" => "a.CreatedAt DESC",
            "duration" => "a.DurationSeconds DESC",
            _ => "a.ReleaseDate DESC"
        };

        if (string.IsNullOrWhiteSpace(sort) || sort.Equals("releaseDate", StringComparison.OrdinalIgnoreCase))
        {
            orderBy = "CASE WHEN a.ReleaseDate IS NULL THEN 1 ELSE 0 END, a.ReleaseDate DESC";
        }

        var countSql = $@"
            SELECT COUNT(1)
            FROM Album AS a
            LEFT JOIN AlbumCategory AS ac ON ac.AlbumCategoryId = a.AlbumCategoryId
            WHERE {whereSql}
            ";

        var dataSql = $@"
            SELECT
                a.AlbumId,
                a.EntityId,
                a.Slug,
                a.Title,
                a.OriginalTitle,
                a.EnglishTitle,
                ac.Name AS CategoryName,
                a.ReleaseDate,
                a.DurationSeconds,
                m.Url AS CoverUrl
            FROM Album AS a
            LEFT JOIN AlbumCategory AS ac ON ac.AlbumCategoryId = a.AlbumCategoryId
            LEFT JOIN Media AS m ON m.MediaId = a.CoverMediaId
            WHERE {whereSql}
            ORDER BY {orderBy}
            {SqlDialect.Pagination()}
            ";

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        connection.Open();

        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, parameters);

        var albumItems = (await connection.QueryAsync<AlbumListItemDto>(dataSql, parameters)).AsList();

        // Spec 7.3: overlay localized titles on list cards (base culture skips).
        if (LocalizationHelper.ShouldLocalize(culture))
        {
            await LocalizeAlbumListAsync(albumItems, culture, cancellationToken);
        }

        return PagedResult<AlbumListItemDto>.Create(albumItems, page, pageSize, totalItems);
    }

    public async Task<AlbumDetailDto?> GetAlbumBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = CacheKeys.Detail("album", culture, slug);

            var cached = await _cache.GetAsync<AlbumDetailDto>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadAlbumDetailCoreAsync(slug, culture, cancellationToken);
            if (result is not null)
            {
                await _cache.SetAsync(cacheKey, result, CacheKeys.DetailDuration, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load album by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private async Task<AlbumDetailDto?> LoadAlbumDetailCoreAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken)
    {
        const string albumSql = @"
            SELECT
                a.AlbumId,
                a.EntityId,
                a.Slug,
                a.Title,
                a.OriginalTitle,
                a.EnglishTitle,
                a.Description,
                ac.Name AS CategoryName,
                a.ReleaseDate,
                a.RecordingStartDate,
                a.RecordingEndDate,
                a.DurationSeconds,
                a.CopyrightNotice,
                m.Url AS CoverUrl
            FROM Album AS a
            LEFT JOIN AlbumCategory AS ac ON ac.AlbumCategoryId = a.AlbumCategoryId
            LEFT JOIN Media AS m ON m.MediaId = a.CoverMediaId
            WHERE a.Slug = @Slug
              AND a.IsDeleted = 0
            ";

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var album = await connection.QuerySingleOrDefaultAsync<AlbumDetailDto>(
                albumSql, new { Slug = slug });

            if (album is null)
                return null;

            var albumId = album.AlbumId;
            var entityId = album.EntityId;

            AlbumDetailDto result;
            {
                var tracksTask = GetAlbumTracksInternalAsync(connection, albumId, cancellationToken);
                var creditsTask = GetAlbumCreditsAsync(connection, entityId, cancellationToken);
                var genresTask = GetAlbumGenresAsync(connection, albumId, cancellationToken);
                var moodsTask = GetAlbumMoodsAsync(connection, albumId, cancellationToken);
                var languagesTask = GetAlbumLanguagesAsync(connection, albumId, cancellationToken);
                var countriesTask = GetAlbumCountriesAsync(connection, albumId, cancellationToken);
                var companiesTask = GetAlbumCompaniesAsync(connection, albumId, cancellationToken);
                var identifiersTask = GetAlbumIdentifiersAsync(connection, albumId, cancellationToken);
                var mediaTask = GetEntityMediaAsync(connection, entityId, cancellationToken);
                var linksTask = GetEntityLinksAsync(connection, entityId, cancellationToken);
                var aliasesTask = GetEntityAliasesAsync(connection, entityId, cancellationToken);
                var tagsTask = GetEntityTagsAsync(connection, entityId, cancellationToken);
                var citationsTask = GetEntityCitationsAsync(connection, entityId, cancellationToken);
                var awardsTask = GetEntityAwardsAsync(connection, entityId, cancellationToken);
                var certificationsTask = GetEntityCertificationsAsync(connection, entityId, cancellationToken);
                var chartEntriesTask = GetEntityChartEntriesAsync(connection, entityId, cancellationToken);
                var relatedAlbumsTask = GetAlbumRelationsAsync(connection, albumId, cancellationToken);
                var sessionsTask = GetAlbumRecordingSessionsAsync(connection, albumId, cancellationToken);
                var eventsTask = GetAlbumPerformanceEventsAsync(connection, albumId, cancellationToken);

                await Task.WhenAll(
                    tracksTask, creditsTask, genresTask, moodsTask,
                    languagesTask, countriesTask, companiesTask, identifiersTask,
                    mediaTask, linksTask, aliasesTask, tagsTask, citationsTask,
                    awardsTask, certificationsTask, chartEntriesTask,
                    relatedAlbumsTask, sessionsTask, eventsTask);

                result = CreateAlbumDetail(album, tracksTask.Result, creditsTask.Result, genresTask.Result,
                    moodsTask.Result, languagesTask.Result, countriesTask.Result,
                    companiesTask.Result, identifiersTask.Result, mediaTask.Result, linksTask.Result,
                    aliasesTask.Result, tagsTask.Result, citationsTask.Result, awardsTask.Result,
                    certificationsTask.Result, chartEntriesTask.Result, relatedAlbumsTask.Result,
                    sessionsTask.Result, eventsTask.Result);
            }

            // Spec 7.3: overlay localized text (requested → base → en → empty)
            if (LocalizationHelper.ShouldLocalize(culture))
            {
                await LocalizeAlbumAsync(result, entityId, culture, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load album by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private static readonly string[] LocalizedAlbumFields =
        ["Title", "OriginalTitle", "EnglishTitle", "Description"];

    private async Task LocalizeAlbumAsync(
        AlbumDetailDto album,
        int entityId,
        string culture,
        CancellationToken cancellationToken)
    {
        var localized = await _localizationService.GetLocalizedValuesAsync(
            entityId, LocalizedAlbumFields, culture, cancellationToken);

        if (localized.Count == 0)
            return;

        album.Title = LocalizationHelper.Pick(localized, "Title", album.Title);
        album.OriginalTitle = LocalizationHelper.Pick(localized, "OriginalTitle", album.OriginalTitle);
        album.EnglishTitle = LocalizationHelper.Pick(localized, "EnglishTitle", album.EnglishTitle);
        album.Description = LocalizationHelper.Pick(localized, "Description", album.Description);
    }

    private async Task LocalizeAlbumListAsync(
        List<AlbumListItemDto> albums,
        string culture,
        CancellationToken cancellationToken)
    {
        var entityIds = albums
            .Select(a => a.EntityId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        if (entityIds.Length == 0)
            return;

        var localized = await _localizationService.GetLocalizedValuesAsync(
            entityIds, ["Title"], culture, cancellationToken);

        if (localized.Count == 0)
            return;

        foreach (var album in albums)
        {
            if (album.EntityId.HasValue &&
                localized.TryGetValue(album.EntityId.Value, out var fields))
            {
                album.Title = LocalizationHelper.Pick(fields, "Title", album.Title);
            }
        }
    }

    private static AlbumDetailDto CreateAlbumDetail(
        AlbumDetailDto album,
        IReadOnlyList<AlbumTrackDto> tracks,
        IReadOnlyList<CreditDto> credits,
        IReadOnlyList<NamedLinkDto> genres,
        IReadOnlyList<NamedLinkDto> moods,
        IReadOnlyList<NamedLinkDto> languages,
        IReadOnlyList<NamedLinkDto> countries,
        IReadOnlyList<AlbumCompanyDto> companies,
        IReadOnlyList<IdentifierDto> identifiers,
        IReadOnlyList<MediaDto> media,
        IReadOnlyList<EntityLinkDto> links,
        IReadOnlyList<AliasDto> aliases,
        IReadOnlyList<TagDto> tags,
        IReadOnlyList<CitationDto> citations,
        IReadOnlyList<AwardAssignmentDto> awards,
        IReadOnlyList<CertificationAssignmentDto> certifications,
        IReadOnlyList<ChartEntryDto> chartEntries,
        IReadOnlyList<AlbumRelationDto> relatedAlbums,
        IReadOnlyList<RecordingSessionDto> sessions,
        IReadOnlyList<PerformanceEventDto> events)
    {
        return new AlbumDetailDto
        {
            AlbumId = album.AlbumId,
            EntityId = album.EntityId,
            Slug = album.Slug,
            Title = album.Title,
            OriginalTitle = album.OriginalTitle,
            EnglishTitle = album.EnglishTitle,
            Description = album.Description,
            CategoryName = album.CategoryName,
            ReleaseDate = album.ReleaseDate,
            RecordingStartDate = album.RecordingStartDate,
            RecordingEndDate = album.RecordingEndDate,
            DurationSeconds = album.DurationSeconds,
            CoverUrl = album.CoverUrl,
            CopyrightNotice = album.CopyrightNotice,
            Tracks = tracks,
            Credits = credits,
            Genres = genres,
            Moods = moods,
            Languages = languages,
            Countries = countries,
            Companies = companies,
            Identifiers = identifiers,
            Media = media,
            Links = links,
            Aliases = aliases,
            Tags = tags,
            Citations = citations,
            Awards = awards,
            Certifications = certifications,
            ChartEntries = chartEntries,
            RelatedAlbums = relatedAlbums,
            RecordingSessions = sessions,
            PerformanceEvents = events
        };
    }

    public async Task<IReadOnlyList<AlbumTrackDto>> GetAlbumTracksAsync(
        int albumId,
        CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        connection.Open();

        return await GetAlbumTracksInternalAsync(connection, albumId, cancellationToken);
    }

    private static async Task<IReadOnlyList<AlbumTrackDto>> GetAlbumTracksInternalAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                at.AlbumTrackId,
                t.TrackId,
                COALESCE(at.TrackTitleOverride, t.Title) AS Title,
                t.Slug,
                at.TrackTitleOverride,
                at.DurationSecondsOverride,
                t.DurationSeconds,
                at.DiscNumber,
                at.TrackNumber,
                at.SequenceNumber,
                at.IsBonus,
                at.IsHidden
            FROM AlbumTrack AS at
            INNER JOIN Track AS t ON t.TrackId = at.TrackId
            WHERE at.AlbumId = @AlbumId
              AND t.IsDeleted = 0
            ORDER BY at.DiscNumber, at.SequenceNumber
            ";

        var trackList = (await connection.QueryAsync<AlbumTrackDto>(sql, new { AlbumId = albumId })).AsList();
        return trackList;
    }

    private static async Task<IReadOnlyList<CreditDto>> GetAlbumCreditsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                c.CreditId,
                c.PersonId,
                p.FullName AS PersonFullName,
                p.Slug AS PersonSlug,
                c.CompanyId,
                co.Name AS CompanyName,
                co.Slug AS CompanySlug,
                cr.Name AS RoleName,
                cr.Code AS RoleCode,
                cr.DisplayOrder AS RoleDisplayOrder,
                i.Name AS InstrumentName,
                c.DisplayOrder,
                c.IsPrimary,
                c.Notes
            FROM Credit AS c
            INNER JOIN CreditRole AS cr ON cr.CreditRoleId = c.CreditRoleId
            LEFT JOIN Person AS p ON p.PersonId = c.PersonId
            LEFT JOIN Company AS co ON co.CompanyId = c.CompanyId
            LEFT JOIN Instrument AS i ON i.InstrumentId = c.InstrumentId
            WHERE c.EntityId = @EntityId
            ORDER BY cr.DisplayOrder, c.DisplayOrder, p.FullName, co.Name
            ";

        var results = await connection.QueryAsync<CreditDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetAlbumGenresAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT g.Slug, g.Name
            FROM AlbumGenre ag
            INNER JOIN Genre g ON g.GenreId = ag.GenreId
            WHERE ag.AlbumId = @AlbumId
            ORDER BY g.Name
            ";
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetAlbumMoodsAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT m.Slug, m.Name
            FROM AlbumMood am
            INNER JOIN Mood m ON m.MoodId = am.MoodId
            WHERE am.AlbumId = @AlbumId
            ORDER BY m.Name
            ";
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetAlbumLanguagesAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT l.Code AS Slug, l.Name
            FROM AlbumLanguage al
            INNER JOIN Language l ON l.LanguageId = al.LanguageId
            WHERE al.AlbumId = @AlbumId
            ORDER BY l.Name
            ";
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetAlbumCountriesAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT c.Code AS Slug, c.Name
            FROM AlbumCountry ac
            INNER JOIN Country c ON c.CountryId = ac.CountryId
            WHERE ac.AlbumId = @AlbumId
            ORDER BY c.Name
            ";
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AlbumCompanyDto>> GetAlbumCompaniesAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                ac.AlbumCompanyId,
                ac.CompanyId,
                co.Name AS CompanyName,
                co.Slug AS CompanySlug,
                crt.Name AS CompanyRole,
                ac.CatalogNumber,
                ac.Barcode
            FROM AlbumCompany ac
            INNER JOIN Company co ON co.CompanyId = ac.CompanyId
            INNER JOIN CompanyRoleType crt ON crt.CompanyRoleTypeId = ac.CompanyRoleTypeId
            WHERE ac.AlbumId = @AlbumId
            ORDER BY crt.Name, co.Name
            ";
        var results = await connection.QueryAsync<AlbumCompanyDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<IdentifierDto>> GetAlbumIdentifiersAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                it.Name AS IdentifierType,
                ai.Value
            FROM AlbumIdentifier ai
            INNER JOIN IdentifierType it ON it.IdentifierTypeId = ai.IdentifierTypeId
            WHERE ai.AlbumId = @AlbumId
            ORDER BY it.Name
            ";
        var results = await connection.QueryAsync<IdentifierDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AlbumRelationDto>> GetAlbumRelationsAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                ar.AlbumRelationId,
                a.AlbumId AS RelatedAlbumId,
                a.Slug,
                a.Title,
                a.ReleaseDate,
                m.Url AS CoverUrl,
                art.Name AS RelationName,
                art.Code AS RelationCode
            FROM AlbumRelation ar
            INNER JOIN Album a ON a.AlbumId = ar.RelatedAlbumId
            LEFT JOIN AlbumRelationType art ON art.AlbumRelationTypeId = ar.AlbumRelationTypeId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE ar.AlbumId = @AlbumId AND a.IsDeleted = 0
            UNION
            SELECT
                ar.AlbumRelationId,
                a.AlbumId AS RelatedAlbumId,
                a.Slug,
                a.Title,
                a.ReleaseDate,
                m.Url AS CoverUrl,
                art.Name AS RelationName,
                art.Code AS RelationCode
            FROM AlbumRelation ar
            INNER JOIN Album a ON a.AlbumId = ar.AlbumId
            LEFT JOIN AlbumRelationType art ON art.AlbumRelationTypeId = ar.AlbumRelationTypeId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE ar.RelatedAlbumId = @AlbumId AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC
            ";
        var results = await connection.QueryAsync<AlbumRelationDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<RecordingSessionDto>> GetAlbumRecordingSessionsAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                rs.RecordingSessionId,
                rs.Slug,
                rs.StartDate,
                rs.EndDate,
                st.Name AS SessionTypeName,
                l.Name AS LocationName,
                rs.Notes
            FROM RecordingSession rs
            INNER JOIN RecordingSessionAlbum rsa ON rsa.RecordingSessionId = rs.RecordingSessionId
            LEFT JOIN SessionType st ON st.SessionTypeId = rs.SessionTypeId
            LEFT JOIN Location l ON l.LocationId = rs.LocationId
            WHERE rsa.AlbumId = @AlbumId AND rs.IsDeleted = 0
            ORDER BY rs.StartDate DESC
            ";
        var results = await connection.QueryAsync<RecordingSessionDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<PerformanceEventDto>> GetAlbumPerformanceEventsAsync(
        IDbConnection connection,
        int albumId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                pe.PerformanceEventId,
                pe.Slug,
                pe.Date,
                et.Name AS EventTypeName,
                l.Name AS LocationName,
                pe.PerformanceNotes
            FROM PerformanceEvent pe
            INNER JOIN PerformanceEventAlbum pea ON pea.PerformanceEventId = pe.PerformanceEventId
            LEFT JOIN EventType et ON et.EventTypeId = pe.EventTypeId
            LEFT JOIN Location l ON l.LocationId = pe.LocationId
            WHERE pea.AlbumId = @AlbumId AND pe.IsDeleted = 0
            ORDER BY pe.Date DESC
            ";
        var results = await connection.QueryAsync<PerformanceEventDto>(sql, new { AlbumId = albumId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<MediaDto>> GetEntityMediaAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                m.MediaId,
                m.Url,
                m.ThumbnailUrl300 AS ThumbnailUrl,
                mt.Name AS MediaType,
                mrt.Name AS MediaRole,
                ma.IsPrimary,
                m.Width,
                m.Height
            FROM MediaAssignment ma
            INNER JOIN Media m ON m.MediaId = ma.MediaId
            INNER JOIN MediaType mt ON mt.MediaTypeId = m.MediaTypeId
            LEFT JOIN MediaRoleType mrt ON mrt.MediaRoleTypeId = ma.MediaRoleTypeId
            WHERE ma.EntityId = @EntityId
              AND m.IsDeleted = 0
            ORDER BY ma.IsPrimary DESC, m.MediaId
            ";
        var results = await connection.QueryAsync<MediaDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<EntityLinkDto>> GetEntityLinksAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                el.EntityLinkId,
                el.Url,
                lt.Name AS LinkType,
                el.Title
            FROM EntityLink el
            LEFT JOIN LinkType lt ON lt.LinkTypeId = el.LinkTypeId
            WHERE el.EntityId = @EntityId
              AND el.IsDeleted = 0
            ORDER BY el.EntityLinkId
            ";
        var results = await connection.QueryAsync<EntityLinkDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AliasDto>> GetEntityAliasesAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                a.AliasId,
                a.AliasName,
                at.Name AS AliasType,
                l.Code AS Language,
                a.IsPrimary,
                a.Notes
            FROM Alias a
            LEFT JOIN AliasType at ON at.AliasTypeId = a.AliasTypeId
            LEFT JOIN Language l ON l.LanguageId = a.LanguageId
            WHERE a.EntityId = @EntityId
            ORDER BY a.IsPrimary DESC, a.AliasId
            ";
        var results = await connection.QueryAsync<AliasDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<TagDto>> GetEntityTagsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT t.TagId, t.Name, t.Slug
            FROM TagAssignment ta
            INNER JOIN Tag t ON t.TagId = ta.TagId
            WHERE ta.EntityId = @EntityId
              AND t.IsDeleted = 0
            ORDER BY t.Name
            ";
        var results = await connection.QueryAsync<TagDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<CitationDto>> GetEntityCitationsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                c.CitationId,
                c.SourceId,
                s.Title AS SourceName,
                s.Slug AS SourceSlug,
                c.FieldName,
                c.Quote,
                c.PageNumber,
                c.Url,
                c.AccessedDate
            FROM Citation c
            LEFT JOIN Source s ON s.SourceId = c.SourceId
            WHERE c.EntityId = @EntityId
            ORDER BY c.CitationId
            ";
        var results = await connection.QueryAsync<CitationDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AwardAssignmentDto>> GetEntityAwardsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                aa.AwardAssignmentId,
                aa.AwardId,
                a.Name AS AwardName,
                a.Slug AS AwardSlug,
                aa.Year,
                art.Name AS Result,
                aa.Category
            FROM AwardAssignment aa
            INNER JOIN Award a ON a.AwardId = aa.AwardId
            LEFT JOIN AwardResultType art ON art.AwardResultTypeId = aa.AwardResultTypeId
            WHERE aa.EntityId = @EntityId
            ORDER BY aa.Year DESC
            ";
        var results = await connection.QueryAsync<AwardAssignmentDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<CertificationAssignmentDto>> GetEntityCertificationsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                ca.CertificationAssignmentId,
                ca.CertificationId,
                c.Name AS CertificationName,
                c.Slug AS CertificationSlug,
                ca.Date AS CertificationDate
            FROM CertificationAssignment ca
            INNER JOIN Certification c ON c.CertificationId = ca.CertificationId
            WHERE ca.EntityId = @EntityId
            ORDER BY ca.Date DESC
            ";
        var results = await connection.QueryAsync<CertificationAssignmentDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<ChartEntryDto>> GetEntityChartEntriesAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT
                ce.ChartEntryId,
                ce.ChartId,
                c.Name AS ChartName,
                c.Slug AS ChartSlug,
                ce.Date,
                ce.Position,
                ce.PreviousPosition,
                ce.WeeksOnChart
            FROM ChartEntry ce
            INNER JOIN Chart c ON c.ChartId = ce.ChartId
            WHERE ce.EntityId = @EntityId
            ORDER BY ce.Date DESC
            ";
        var results = await connection.QueryAsync<ChartEntryDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }
}
