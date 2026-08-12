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

/// <summary>
/// Provides track query operations using Dapper.
/// </summary>
public sealed class TrackQueryService : ITrackQueryService
{
    private readonly string _connectionString;
    private readonly bool _isSqlite;
    private readonly ILogger<TrackQueryService> _logger;
    private readonly IContentLocalizationService _localizationService;
    private readonly ICacheService _cache;

    public TrackQueryService(
        IConfiguration configuration,
        ILogger<TrackQueryService> logger,
        IContentLocalizationService localizationService,
        ICacheService cache)
    {
        _localizationService = localizationService;
        _cache = cache;

        var dbProvider = configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
        _isSqlite = string.Equals(dbProvider, "Sqlite", StringComparison.OrdinalIgnoreCase);

        _connectionString = _isSqlite
            ? configuration.GetConnectionString("SqliteConnection")
                ?? throw new InvalidOperationException("Connection string 'SqliteConnection' not found.")
            : configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    private IDbConnection CreateConnection()
    {
        return _isSqlite
            ? (IDbConnection)new Microsoft.Data.Sqlite.SqliteConnection(_connectionString)
            : new SqlConnection(_connectionString);
    }

    /// <inheritdoc />
    public async Task<PagedResult<TrackDetailDto>> GetTracksAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? genre = null,
        string? mood = null,
        string? artist = null,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var cacheKey = CacheKeys.List("track", culture, page, pageSize, sort, genre, mood, artist, q);

            var cached = await _cache.GetAsync<PagedResult<TrackDetailDto>>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadTracksCoreAsync(
                culture, page, pageSize, sort, genre, mood, artist, q, cancellationToken);

            await _cache.SetAsync(cacheKey, result, CacheKeys.ListDuration, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load track list for culture={Culture}, page={Page}", culture, page);
            return PagedResult<TrackDetailDto>.Create([], page, pageSize, 0);
        }
    }

    private async Task<PagedResult<TrackDetailDto>> LoadTracksCoreAsync(
        string culture,
        int page,
        int pageSize,
        string? sort,
        string? genre,
        string? mood,
        string? artist,
        string? q,
        CancellationToken cancellationToken)
    {
        var whereClauses = new List<string> { "t.IsDeleted = 0" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(genre))
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM TrackGenre tg_inner JOIN Genre g_inner ON g_inner.GenreId = tg_inner.GenreId WHERE tg_inner.TrackId = t.TrackId AND g_inner.Slug = @Genre)");
            parameters.Add("Genre", genre);
        }

        if (!string.IsNullOrWhiteSpace(mood))
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM TrackMood tm_inner JOIN Mood m_inner ON m_inner.MoodId = tm_inner.MoodId WHERE tm_inner.TrackId = t.TrackId AND m_inner.Slug = @Mood)");
            parameters.Add("Mood", mood);
        }

        if (!string.IsNullOrWhiteSpace(artist))
        {
            whereClauses.Add("EXISTS (SELECT 1 FROM Credit c_inner INNER JOIN Entity e_inner ON e_inner.EntityId = c_inner.EntityId INNER JOIN Person p_inner ON p_inner.PersonId = c_inner.PersonId WHERE e_inner.EntityId = t.TrackId AND p_inner.Slug = @Artist)");
            parameters.Add("Artist", artist);
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            whereClauses.Add("(t.Title LIKE @Q OR t.OriginalTitle LIKE @Q OR t.EnglishTitle LIKE @Q)");
            parameters.Add("Q", $"%{q}%");
        }

        var whereSql = string.Join(" AND ", whereClauses);

        var orderBy = sort?.ToLowerInvariant() switch
        {
            "title" => "t.Title ASC",
            "duration" => "t.DurationSeconds DESC",
            "createdat" => "t.CreatedAt DESC",
            _ => "t.CreatedAt DESC"
        };

        // For release date sorting, join with album track
        if (string.IsNullOrWhiteSpace(sort) || sort.Equals("releaseDate", StringComparison.OrdinalIgnoreCase))
        {
            orderBy = """
                (
                    SELECT MIN(a.ReleaseDate)
                    FROM AlbumTrack at_sub
                    INNER JOIN Album a ON a.AlbumId = at_sub.AlbumId
                    WHERE at_sub.TrackId = t.TrackId AND a.IsDeleted = 0
                ) DESC
                """;
        }

        var countSql = $"""
            SELECT COUNT(1)
            FROM Track AS t
            WHERE {whereSql}
            """;

        var dataSql = $"""
            SELECT
                t.TrackId,
                t.EntityId,
                t.Slug,
                t.Title,
                t.OriginalTitle,
                t.EnglishTitle,
                t.DurationSeconds,
                t.IsInstrumental,
                t.IsExplicit,
                t.Isrc,
                t.Bpm,
                mk.Name AS MusicalKeyName,
                vs.Name AS VocalStyleName,
                lat.Name AS LyricsAvailabilityName
            FROM Track AS t
            LEFT JOIN MusicalKey AS mk ON mk.MusicalKeyId = t.MusicalKeyId
            LEFT JOIN VocalStyle AS vs ON vs.VocalStyleId = t.VocalStyleId
            LEFT JOIN LyricsAvailabilityType AS lat ON lat.LyricsAvailabilityTypeId = t.LyricsAvailabilityTypeId
            WHERE {whereSql}
            ORDER BY {orderBy}
            {SqlDialect.Pagination(_isSqlite)}
            """;

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        connection.Open();

        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<TrackDetailDto>(dataSql, parameters);

        return PagedResult<TrackDetailDto>.Create(items.AsList(), page, pageSize, totalItems);
    }

    /// <inheritdoc />
    public async Task<TrackDetailDto?> GetTrackBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = CacheKeys.Detail("track", culture, slug);

            var cached = await _cache.GetAsync<TrackDetailDto>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadTrackDetailCoreAsync(slug, culture, cancellationToken);
            if (result is not null)
            {
                await _cache.SetAsync(cacheKey, result, CacheKeys.DetailDuration, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load track by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private async Task<TrackDetailDto?> LoadTrackDetailCoreAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken)
    {
        const string trackSql = """
            SELECT
                t.TrackId,
                t.EntityId,
                t.Slug,
                t.Title,
                t.OriginalTitle,
                t.EnglishTitle,
                t.DurationSeconds,
                t.IsInstrumental,
                t.IsExplicit,
                t.Isrc,
                t.Bpm,
                mk.Name AS MusicalKeyName,
                vs.Name AS VocalStyleName,
                lat.Name AS LyricsAvailabilityName
            FROM Track AS t
            LEFT JOIN MusicalKey AS mk ON mk.MusicalKeyId = t.MusicalKeyId
            LEFT JOIN VocalStyle AS vs ON vs.VocalStyleId = t.VocalStyleId
            LEFT JOIN LyricsAvailabilityType AS lat ON lat.LyricsAvailabilityTypeId = t.LyricsAvailabilityTypeId
            WHERE t.Slug = @Slug
              AND t.IsDeleted = 0
            """;

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var track = await connection.QuerySingleOrDefaultAsync<TrackDetailDto>(
                trackSql, new { Slug = slug });

            if (track is null)
                return null;

            var trackId = track.TrackId;
            var entityId = track.EntityId;

            TrackDetailDto result;

            if (_isSqlite)
            {
                var albums = await GetTrackAlbumsAsync(connection, trackId, cancellationToken);
                var credits = await GetTrackCreditsAsync(connection, entityId, cancellationToken);
                var musicians = await GetTrackMusiciansAsync(connection, entityId, cancellationToken);
                var genres = await GetTrackGenresAsync(connection, trackId, cancellationToken);
                var moods = await GetTrackMoodsAsync(connection, trackId, cancellationToken);
                var instruments = await GetTrackInstrumentsAsync(connection, trackId, cancellationToken);
                var media = await GetEntityMediaAsync(connection, entityId, cancellationToken);
                var links = await GetEntityLinksAsync(connection, entityId, cancellationToken);
                var citations = await GetEntityCitationsAsync(connection, entityId, cancellationToken);
                var tags = await GetEntityTagsAsync(connection, entityId, cancellationToken);
                var aliases = await GetEntityAliasesAsync(connection, entityId, cancellationToken);
                var awards = await GetEntityAwardsAsync(connection, entityId, cancellationToken);
                var certifications = await GetEntityCertificationsAsync(connection, entityId, cancellationToken);
                var chartEntries = await GetEntityChartEntriesAsync(connection, entityId, cancellationToken);
                var relatedTracks = await GetTrackRelationsAsync(connection, trackId, cancellationToken);
                var sessions = await GetTrackRecordingSessionsAsync(connection, trackId, cancellationToken);
                var events = await GetTrackPerformanceEventsAsync(connection, trackId, cancellationToken);

                result = CreateTrackDetail(track, albums, credits, musicians, genres, moods, instruments,
                    media, links, citations, tags, aliases, awards, certifications, chartEntries,
                    relatedTracks, sessions, events);
            }
            else
            {
                var albumsTask = GetTrackAlbumsAsync(connection, trackId, cancellationToken);
                var creditsTask = GetTrackCreditsAsync(connection, entityId, cancellationToken);
                var musiciansTask = GetTrackMusiciansAsync(connection, entityId, cancellationToken);
                var genresTask = GetTrackGenresAsync(connection, trackId, cancellationToken);
                var moodsTask = GetTrackMoodsAsync(connection, trackId, cancellationToken);
                var instrumentsTask = GetTrackInstrumentsAsync(connection, trackId, cancellationToken);
                var mediaTask = GetEntityMediaAsync(connection, entityId, cancellationToken);
                var linksTask = GetEntityLinksAsync(connection, entityId, cancellationToken);
                var citationsTask = GetEntityCitationsAsync(connection, entityId, cancellationToken);
                var tagsTask = GetEntityTagsAsync(connection, entityId, cancellationToken);
                var aliasesTask = GetEntityAliasesAsync(connection, entityId, cancellationToken);
                var awardsTask = GetEntityAwardsAsync(connection, entityId, cancellationToken);
                var certificationsTask = GetEntityCertificationsAsync(connection, entityId, cancellationToken);
                var chartEntriesTask = GetEntityChartEntriesAsync(connection, entityId, cancellationToken);
                var relatedTracksTask = GetTrackRelationsAsync(connection, trackId, cancellationToken);
                var sessionsTask = GetTrackRecordingSessionsAsync(connection, trackId, cancellationToken);
                var eventsTask = GetTrackPerformanceEventsAsync(connection, trackId, cancellationToken);

                await Task.WhenAll(
                    albumsTask, creditsTask, musiciansTask, genresTask, moodsTask,
                    instrumentsTask, mediaTask, linksTask, citationsTask, tagsTask,
                    aliasesTask, awardsTask, certificationsTask, chartEntriesTask,
                    relatedTracksTask, sessionsTask, eventsTask);

                result = CreateTrackDetail(track, albumsTask.Result, creditsTask.Result, musiciansTask.Result,
                    genresTask.Result, moodsTask.Result, instrumentsTask.Result,
                    mediaTask.Result, linksTask.Result, citationsTask.Result, tagsTask.Result,
                    aliasesTask.Result, awardsTask.Result, certificationsTask.Result, chartEntriesTask.Result,
                    relatedTracksTask.Result, sessionsTask.Result, eventsTask.Result);
            }

            // Spec 7.3: overlay localized text (requested → base → en → empty)
            if (LocalizationHelper.ShouldLocalize(culture))
            {
                await LocalizeTrackAsync(result, entityId, culture, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load track by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private static readonly string[] LocalizedTrackFields =
        ["Title", "OriginalTitle", "EnglishTitle"];

    private async Task LocalizeTrackAsync(
        TrackDetailDto track,
        int entityId,
        string culture,
        CancellationToken cancellationToken)
    {
        var localized = await _localizationService.GetLocalizedValuesAsync(
            entityId, LocalizedTrackFields, culture, cancellationToken);

        if (localized.Count == 0)
            return;

        track.Title = LocalizationHelper.Pick(localized, "Title", track.Title);
        track.OriginalTitle = LocalizationHelper.Pick(localized, "OriginalTitle", track.OriginalTitle);
        track.EnglishTitle = LocalizationHelper.Pick(localized, "EnglishTitle", track.EnglishTitle);
    }

    private static TrackDetailDto CreateTrackDetail(
        TrackDetailDto track,
        IReadOnlyList<TrackAlbumAppearanceDto> albums,
        IReadOnlyList<CreditDto> credits,
        IReadOnlyList<MusicianCreditDto> musicians,
        IReadOnlyList<NamedLinkDto> genres,
        IReadOnlyList<NamedLinkDto> moods,
        IReadOnlyList<NamedLinkDto> instruments,
        IReadOnlyList<MediaDto> media,
        IReadOnlyList<EntityLinkDto> links,
        IReadOnlyList<CitationDto> citations,
        IReadOnlyList<TagDto> tags,
        IReadOnlyList<AliasDto> aliases,
        IReadOnlyList<AwardAssignmentDto> awards,
        IReadOnlyList<CertificationAssignmentDto> certifications,
        IReadOnlyList<ChartEntryDto> chartEntries,
        IReadOnlyList<TrackRelationDto> relatedTracks,
        IReadOnlyList<RecordingSessionDto> sessions,
        IReadOnlyList<PerformanceEventDto> events)
    {
        return new TrackDetailDto
        {
            TrackId = track.TrackId,
            EntityId = track.EntityId,
            Slug = track.Slug,
            Title = track.Title,
            OriginalTitle = track.OriginalTitle,
            EnglishTitle = track.EnglishTitle,
            DurationSeconds = track.DurationSeconds,
            IsInstrumental = track.IsInstrumental,
            IsExplicit = track.IsExplicit,
            Isrc = track.Isrc,
            Bpm = track.Bpm,
            MusicalKeyName = track.MusicalKeyName,
            VocalStyleName = track.VocalStyleName,
            LyricsAvailabilityName = track.LyricsAvailabilityName,
            Albums = albums,
            Credits = credits,
            Musicians = musicians,
            Genres = genres,
            Moods = moods,
            Instruments = instruments,
            Media = media,
            Links = links,
            Citations = citations,
            Tags = tags,
            Aliases = aliases,
            Awards = awards,
            Certifications = certifications,
            ChartEntries = chartEntries,
            RelatedTracks = relatedTracks,
            RecordingSessions = sessions,
            PerformanceEvents = events
        };
    }

    private static async Task<IReadOnlyList<TrackAlbumAppearanceDto>> GetTrackAlbumsAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.AlbumId,
                a.Title AS AlbumTitle,
                a.Slug AS AlbumSlug,
                ac.Name AS CategoryName,
                m.Url AS CoverUrl,
                at.DiscNumber,
                at.TrackNumber,
                at.SequenceNumber,
                a.ReleaseDate,
                at.DurationSecondsOverride,
                at.TrackTitleOverride
            FROM AlbumTrack at
            INNER JOIN Album a ON a.AlbumId = at.AlbumId
            LEFT JOIN AlbumCategory ac ON ac.AlbumCategoryId = a.AlbumCategoryId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE at.TrackId = @TrackId
              AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC, at.DiscNumber, at.SequenceNumber
            """;
        var results = await connection.QueryAsync<TrackAlbumAppearanceDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<CreditDto>> GetTrackCreditsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<CreditDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<MusicianCreditDto>> GetTrackMusiciansAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.CreditId,
                c.PersonId,
                p.FullName AS PersonFullName,
                p.Slug AS PersonSlug,
                i.Name AS InstrumentName,
                i.InstrumentId,
                c.Notes
            FROM Credit AS c
            INNER JOIN CreditRole AS cr ON cr.CreditRoleId = c.CreditRoleId
            INNER JOIN Person AS p ON p.PersonId = c.PersonId
            LEFT JOIN Instrument AS i ON i.InstrumentId = c.InstrumentId
            WHERE c.EntityId = @EntityId
              AND cr.Code = 'MUSICIAN'
            ORDER BY c.DisplayOrder, p.FullName, i.Name
            """;
        var results = await connection.QueryAsync<MusicianCreditDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetTrackGenresAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT g.Slug, g.Name
            FROM TrackGenre tg
            INNER JOIN Genre g ON g.GenreId = tg.GenreId
            WHERE tg.TrackId = @TrackId
            ORDER BY g.Name
            """;
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetTrackMoodsAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT m.Slug, m.Name
            FROM TrackMood tm
            INNER JOIN Mood m ON m.MoodId = tm.MoodId
            WHERE tm.TrackId = @TrackId
            ORDER BY m.Name
            """;
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetTrackInstrumentsAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT i.Slug, i.Name
            FROM TrackInstrument ti
            INNER JOIN Instrument i ON i.InstrumentId = ti.InstrumentId
            WHERE ti.TrackId = @TrackId
            ORDER BY i.Name
            """;
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<TrackRelationDto>> GetTrackRelationsAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                tr.TrackRelationId,
                t.TrackId AS RelatedTrackId,
                t.Slug,
                t.Title,
                t.DurationSeconds,
                trt.Name AS RelationName,
                trt.Code AS RelationCode
            FROM TrackRelation tr
            INNER JOIN Track t ON t.TrackId = tr.RelatedTrackId
            LEFT JOIN TrackRelationType trt ON trt.TrackRelationTypeId = tr.TrackRelationTypeId
            WHERE tr.TrackId = @TrackId AND t.IsDeleted = 0
            UNION
            SELECT
                tr.TrackRelationId,
                t.TrackId AS RelatedTrackId,
                t.Slug,
                t.Title,
                t.DurationSeconds,
                trt.Name AS RelationName,
                trt.Code AS RelationCode
            FROM TrackRelation tr
            INNER JOIN Track t ON t.TrackId = tr.TrackId
            LEFT JOIN TrackRelationType trt ON trt.TrackRelationTypeId = tr.TrackRelationTypeId
            WHERE tr.RelatedTrackId = @TrackId AND t.IsDeleted = 0
            ORDER BY t.Title
            """;
        var results = await connection.QueryAsync<TrackRelationDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<RecordingSessionDto>> GetTrackRecordingSessionsAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                rs.RecordingSessionId,
                rs.Slug,
                rs.StartDate,
                rs.EndDate,
                st.Name AS SessionTypeName,
                l.Name AS LocationName,
                rs.Notes
            FROM RecordingSession rs
            INNER JOIN RecordingSessionTrack rst ON rst.RecordingSessionId = rs.RecordingSessionId
            LEFT JOIN SessionType st ON st.SessionTypeId = rs.SessionTypeId
            LEFT JOIN Location l ON l.LocationId = rs.LocationId
            WHERE rst.TrackId = @TrackId AND rs.IsDeleted = 0
            ORDER BY rs.StartDate DESC
            """;
        var results = await connection.QueryAsync<RecordingSessionDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<PerformanceEventDto>> GetTrackPerformanceEventsAsync(
        IDbConnection connection,
        int trackId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                pe.PerformanceEventId,
                pe.Slug,
                pe.Date,
                et.Name AS EventTypeName,
                l.Name AS LocationName,
                pe.PerformanceNotes
            FROM PerformanceEvent pe
            INNER JOIN PerformanceEventTrack pet ON pet.PerformanceEventId = pe.PerformanceEventId
            LEFT JOIN EventType et ON et.EventTypeId = pe.EventTypeId
            LEFT JOIN Location l ON l.LocationId = pe.LocationId
            WHERE pet.TrackId = @TrackId AND pe.IsDeleted = 0
            ORDER BY pe.Date DESC
            """;
        var results = await connection.QueryAsync<PerformanceEventDto>(sql, new { TrackId = trackId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<MediaDto>> GetEntityMediaAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<MediaDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<EntityLinkDto>> GetEntityLinksAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<EntityLinkDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<CitationDto>> GetEntityCitationsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<CitationDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<TagDto>> GetEntityTagsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT t.TagId, t.Name, t.Slug
            FROM TagAssignment ta
            INNER JOIN Tag t ON t.TagId = ta.TagId
            WHERE ta.EntityId = @EntityId
              AND t.IsDeleted = 0
            ORDER BY t.Name
            """;
        var results = await connection.QueryAsync<TagDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AliasDto>> GetEntityAliasesAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<AliasDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AwardAssignmentDto>> GetEntityAwardsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<AwardAssignmentDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<CertificationAssignmentDto>> GetEntityCertificationsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<CertificationAssignmentDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<ChartEntryDto>> GetEntityChartEntriesAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
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
            """;
        var results = await connection.QueryAsync<ChartEntryDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }
}
