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
/// Provides person query operations using Dapper.
/// </summary>
public sealed class PersonQueryService : IPersonQueryService
{
    private readonly string _connectionString;
    private readonly ILogger<PersonQueryService> _logger;
    private readonly IContentLocalizationService _localizationService;
    private readonly ICacheService _cache;

    public PersonQueryService(
        IConfiguration configuration,
        ILogger<PersonQueryService> logger,
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

    /// <inheritdoc />
    public async Task<PagedResult<NamedLinkDto>> GetPeopleAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? q = null,
        string? personType = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var cacheKey = CacheKeys.List("person", culture, page, pageSize, sort, q, personType);

            var cached = await _cache.GetAsync<PagedResult<NamedLinkDto>>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadPeopleCoreAsync(
                culture, page, pageSize, sort, q, personType, cancellationToken);

            await _cache.SetAsync(cacheKey, result, CacheKeys.ListDuration, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load people list for culture={Culture}, page={Page}", culture, page);
            return PagedResult<NamedLinkDto>.Create([], page, pageSize, 0);
        }
    }

    private async Task<PagedResult<NamedLinkDto>> LoadPeopleCoreAsync(
        string culture,
        int page,
        int pageSize,
        string? sort,
        string? q,
        string? personType,
        CancellationToken cancellationToken)
    {
        var whereClauses = new List<string> { "p.IsDeleted = 0" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(q))
        {
            whereClauses.Add("(p.FullName LIKE @Q OR p.OriginalName LIKE @Q OR p.EnglishName LIKE @Q)");
            parameters.Add("Q", $"%{q}%");
        }

        if (!string.IsNullOrWhiteSpace(personType))
        {
            whereClauses.Add("""
                EXISTS (
                    SELECT 1
                    FROM PersonTypeAssignment pta
                    INNER JOIN PersonType pt ON pt.PersonTypeId = pta.PersonTypeId
                    WHERE pta.PersonId = p.PersonId
                      AND pt.Code = @PersonType
                )
                """);
            parameters.Add("PersonType", personType);
        }

        var whereSql = string.Join(" AND ", whereClauses);

        var orderBy = sort?.ToLowerInvariant() switch
        {
            "name" => "p.FullName ASC",
            "createdat" => "p.CreatedAt DESC",
            _ => "p.FullName ASC"
        };

        var countSql = $"""
            SELECT COUNT(1)
            FROM Person AS p
            WHERE {whereSql}
            """;

        var dataSql = $"""
            SELECT
                p.EntityId,
                p.Slug,
                p.FullName AS Name
            FROM Person AS p
            WHERE {whereSql}
            ORDER BY {orderBy}
            {SqlDialect.Pagination()}
            """;

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        connection.Open();

        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = (await connection.QueryAsync<NamedLinkDto>(dataSql, parameters)).AsList();

        // Spec 7.3: overlay localized names on list cards (base culture skips).
        if (LocalizationHelper.ShouldLocalize(culture))
        {
            await LocalizePeopleListAsync(items, culture, cancellationToken);
        }

        return PagedResult<NamedLinkDto>.Create(items, page, pageSize, totalItems);
    }

    /// <inheritdoc />
    public async Task<PersonDetailDto?> GetPersonBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = CacheKeys.Detail("person", culture, slug);

            var cached = await _cache.GetAsync<PersonDetailDto>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadPersonDetailCoreAsync(slug, culture, cancellationToken);
            if (result is not null)
            {
                await _cache.SetAsync(cacheKey, result, CacheKeys.DetailDuration, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load person by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private async Task<PersonDetailDto?> LoadPersonDetailCoreAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken)
    {
        const string personSql = """
            SELECT
                p.PersonId,
                p.EntityId,
                p.Slug,
                p.FullName,
                p.OriginalName,
                p.EnglishName,
                pk.Name AS PersonKind,
                p.Biography,
                p.BirthDate,
                p.BirthDatePrecision,
                p.DeathDate,
                p.DeathDatePrecision,
                nc.Name AS Nationality,
                m.Url AS ImageUrl
            FROM Person AS p
            LEFT JOIN PersonKind AS pk ON pk.PersonKindId = p.PersonKindId
            LEFT JOIN Country AS nc ON nc.CountryId = p.NationalityCountryId
            LEFT JOIN Media AS m ON m.MediaId = p.ImageMediaId
            WHERE p.Slug = @Slug
              AND p.IsDeleted = 0
            """;

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var person = await connection.QuerySingleOrDefaultAsync<PersonDetailDto>(
                personSql, new { Slug = slug });

            if (person is null)
                return null;

            var entityId = person.EntityId;
            var personId = person.PersonId;

            // Spec 7.3: fetch localized text (requested + English) in parallel with
            // the detail sub-queries, using its own connection.
            Task<IReadOnlyDictionary<string, LocalizedFieldValues>>? localizationTask = null;
            if (LocalizationHelper.ShouldLocalize(culture))
            {
                localizationTask = _localizationService.GetLocalizedValuesAsync(
                    entityId, LocalizedPersonFields, culture, cancellationToken);
            }

            // ── Batch 1: classic sections + discography / contributions ──
            IReadOnlyList<MediaDto> media;
            IReadOnlyList<EntityLinkDto> links;
            IReadOnlyList<AliasDto> aliases;
            IReadOnlyList<TagDto> tags;
            IReadOnlyList<CitationDto> citations;
            IReadOnlyList<NamedLinkDto> instruments;
            IReadOnlyList<NamedLinkDto> roles;
            IReadOnlyList<PersonAlbumDto> albums;
            IReadOnlyList<AlbumCreditRow> albumCredits;
            IReadOnlyList<TrackContributionDto> contributions;
            IReadOnlyList<TrackCreditRow> trackCredits;

            {
                var mediaTask = GetEntityMediaAsync(connection, entityId, cancellationToken);
                var linksTask = GetEntityLinksAsync(connection, entityId, cancellationToken);
                var aliasesTask = GetEntityAliasesAsync(connection, entityId, cancellationToken);
                var tagsTask = GetEntityTagsAsync(connection, entityId, cancellationToken);
                var citationsTask = GetEntityCitationsAsync(connection, entityId, cancellationToken);
                var instrumentsTask = GetPersonInstrumentsAsync(connection, personId, cancellationToken);
                var rolesTask = GetPersonRolesAsync(connection, personId, cancellationToken);
                var albumsTask = GetPersonAlbumsAsync(connection, personId, cancellationToken);
                var albumCreditsTask = GetPersonAlbumCreditsAsync(connection, personId, cancellationToken);
                var contributionsTask = GetPersonTrackContributionsAsync(connection, personId, cancellationToken);
                var trackCreditsTask = GetPersonTrackCreditsAsync(connection, personId, cancellationToken);

                var tasks = new List<Task>
                {
                    mediaTask, linksTask, aliasesTask, tagsTask, citationsTask,
                    instrumentsTask, rolesTask, albumsTask, albumCreditsTask,
                    contributionsTask, trackCreditsTask
                };
                if (localizationTask is not null)
                    tasks.Add(localizationTask);

                await Task.WhenAll(tasks);

                media = mediaTask.Result;
                links = linksTask.Result;
                aliases = aliasesTask.Result;
                tags = tagsTask.Result;
                citations = citationsTask.Result;
                instruments = instrumentsTask.Result;
                roles = rolesTask.Result;
                albums = albumsTask.Result;
                albumCredits = albumCreditsTask.Result;
                contributions = contributionsTask.Result;
                trackCredits = trackCreditsTask.Result;
            }

            // Aggregate the person's roles/instruments onto the discography rows.
            var albumsById = albums.ToDictionary(a => a.AlbumId);
            foreach (var credit in albumCredits)
            {
                if (albumsById.TryGetValue(credit.AlbumId, out var album))
                {
                    ApplyCreditToAlbum(album, credit);
                }
            }

            var contributionsByTrack = contributions
                .GroupBy(c => c.TrackId)
                .ToDictionary(g => g.Key, g => g.ToList());
            foreach (var credit in trackCredits)
            {
                if (contributionsByTrack.TryGetValue(credit.TrackId, out var rows))
                {
                    foreach (var row in rows)
                    {
                        ApplyCreditToContribution(row, credit);
                    }
                }
            }

            // ── Batch 2: timeline sources — needs album/track ids from batch 1 ──
            var albumIds = albums.Select(a => a.AlbumId).ToArray();
            var trackIds = contributions.Select(c => c.TrackId).Distinct().ToArray();

            IReadOnlyDictionary<int, IReadOnlyList<string>> trackGenres;
            IReadOnlyList<RecordingSessionDto> sessions;
            IReadOnlyList<PerformanceEventDto> events;
            IReadOnlyList<AwardTimelineRow> awards;
            IReadOnlyList<ChartTimelineRow> chartRows;
            IReadOnlyList<PublicationTimelineRow> publications;

            {
                var genresTask = trackIds.Length > 0
                    ? GetTrackGenresMapAsync(connection, trackIds, cancellationToken)
                    : Task.FromResult<IReadOnlyDictionary<int, IReadOnlyList<string>>>(
                        new Dictionary<int, IReadOnlyList<string>>());
                var sessionsTask = GetPersonSessionsAsync(connection, albumIds, trackIds, cancellationToken);
                var eventsTask = GetPersonEventsAsync(connection, albumIds, trackIds, cancellationToken);
                var awardsTask = GetPersonAwardsAsync(connection, albumIds, cancellationToken);
                var chartsTask = GetPersonChartsAsync(connection, albumIds, cancellationToken);
                var publicationsTask = GetPersonPublicationsAsync(connection, personId, cancellationToken);

                await Task.WhenAll(
                    genresTask, sessionsTask, eventsTask, awardsTask, chartsTask, publicationsTask);

                trackGenres = genresTask.Result;
                sessions = sessionsTask.Result;
                events = eventsTask.Result;
                awards = awardsTask.Result;
                chartRows = chartsTask.Result;
                publications = publicationsTask.Result;
            }

            var localized = localizationTask is null
                ? (IReadOnlyDictionary<string, LocalizedFieldValues>)
                    new Dictionary<string, LocalizedFieldValues>(StringComparer.OrdinalIgnoreCase)
                : await localizationTask;

            // Attach genre names to each contribution (filter dimension).
            foreach (var contribution in contributions)
            {
                if (trackGenres.TryGetValue(contribution.TrackId, out var genres))
                {
                    contribution.Genres = genres;
                }
            }

            // Spec 7.3: overlay localized titles on the discography / contributions.
            if (LocalizationHelper.ShouldLocalize(culture))
            {
                await LocalizeDiscographyAsync(
                    albums.ToList(), contributions.ToList(), culture, cancellationToken);
            }

            return CreatePersonDetail(
                person, localized, media, links, aliases, tags, citations,
                instruments, roles, albums, contributions, trackGenres,
                sessions, events, awards, chartRows, publications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load person by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private static readonly string[] LocalizedPersonFields =
        ["Name", "OriginalName", "EnglishName", "Biography"];

    private async Task LocalizePeopleListAsync(
        List<NamedLinkDto> people,
        string culture,
        CancellationToken cancellationToken)
    {
        var entityIds = people
            .Select(p => p.EntityId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        if (entityIds.Length == 0)
            return;

        var localized = await _localizationService.GetLocalizedValuesAsync(
            entityIds, ["Name"], culture, cancellationToken);

        if (localized.Count == 0)
            return;

        foreach (var person in people)
        {
            if (person.EntityId.HasValue &&
                localized.TryGetValue(person.EntityId.Value, out var fields))
            {
                person.Name = LocalizationHelper.Pick(fields, "Name", person.Name);
            }
        }
    }

    private async Task LocalizeDiscographyAsync(
        List<PersonAlbumDto> albums,
        List<TrackContributionDto> contributions,
        string culture,
        CancellationToken cancellationToken)
    {
        var albumEntityIds = albums.Select(a => a.EntityId).Distinct().ToArray();
        if (albumEntityIds.Length > 0)
        {
            var localized = await _localizationService.GetLocalizedValuesAsync(
                albumEntityIds, ["Title"], culture, cancellationToken);
            foreach (var album in albums)
            {
                if (localized.TryGetValue(album.EntityId, out var fields))
                    album.Title = LocalizationHelper.Pick(fields, "Title", album.Title);
            }
        }

        var trackEntityIds = contributions.Select(c => c.EntityId).Distinct().ToArray();
        if (trackEntityIds.Length > 0)
        {
            var localized = await _localizationService.GetLocalizedValuesAsync(
                trackEntityIds, ["Title"], culture, cancellationToken);
            foreach (var contribution in contributions)
            {
                if (localized.TryGetValue(contribution.EntityId, out var fields))
                    contribution.Title = LocalizationHelper.Pick(fields, "Title", contribution.Title);
            }
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Phase 4 queries
    // ────────────────────────────────────────────────────────────────

    private static void ApplyCreditToAlbum(PersonAlbumDto album, AlbumCreditRow credit)
    {
        if (!string.IsNullOrWhiteSpace(credit.RoleName) &&
            !album.Roles.Contains(credit.RoleName))
        {
            album.Roles = album.Roles.Concat(new[] { credit.RoleName }).OrderBy(r => r).ToList();
        }
        if (!string.IsNullOrWhiteSpace(credit.InstrumentName) &&
            !album.Instruments.Contains(credit.InstrumentName))
        {
            album.Instruments = album.Instruments.Concat(new[] { credit.InstrumentName }).OrderBy(i => i).ToList();
        }
    }

    private static void ApplyCreditToContribution(TrackContributionDto contribution, TrackCreditRow credit)
    {
        if (!string.IsNullOrWhiteSpace(credit.RoleName) &&
            !contribution.Roles.Contains(credit.RoleName))
        {
            contribution.Roles = contribution.Roles.Concat(new[] { credit.RoleName }).OrderBy(r => r).ToList();
        }
        if (!string.IsNullOrWhiteSpace(credit.InstrumentName) &&
            !contribution.Instruments.Contains(credit.InstrumentName))
        {
            contribution.Instruments = contribution.Instruments.Concat(new[] { credit.InstrumentName }).OrderBy(i => i).ToList();
        }
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetPersonInstrumentsAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT i.Slug, i.Name
            FROM MusicianInstrument mi
            INNER JOIN Instrument i ON i.InstrumentId = mi.InstrumentId
            WHERE mi.PersonId = @PersonId AND i.IsDeleted = 0
            UNION
            SELECT DISTINCT i.Slug, i.Name
            FROM Credit c
            INNER JOIN Instrument i ON i.InstrumentId = c.InstrumentId
            WHERE c.PersonId = @PersonId AND i.IsDeleted = 0
            ORDER BY Name
            """;
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<NamedLinkDto>> GetPersonRolesAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT cr.Code AS Slug, cr.Name
            FROM Credit c
            INNER JOIN CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
            WHERE c.PersonId = @PersonId
            ORDER BY cr.Name
            """;
        var results = await connection.QueryAsync<NamedLinkDto>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<PersonAlbumDto>> GetPersonAlbumsAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT
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
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN Album a ON a.EntityId = e.EntityId
            LEFT JOIN AlbumCategory ac ON ac.AlbumCategoryId = a.AlbumCategoryId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE c.PersonId = @PersonId
              AND et.Code = 'Album'
              AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC
            """;
        var results = await connection.QueryAsync<PersonAlbumDto>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AlbumCreditRow>> GetPersonAlbumCreditsAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.AlbumId,
                cr.Name AS RoleName,
                i.Name AS InstrumentName
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN Album a ON a.EntityId = e.EntityId
            LEFT JOIN CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
            LEFT JOIN Instrument i ON i.InstrumentId = c.InstrumentId
            WHERE c.PersonId = @PersonId
              AND et.Code = 'Album'
              AND a.IsDeleted = 0
            """;
        var results = await connection.QueryAsync<AlbumCreditRow>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<TrackContributionDto>> GetPersonTrackContributionsAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT
                t.TrackId,
                t.EntityId,
                t.Slug,
                t.Title,
                t.OriginalTitle,
                t.EnglishTitle,
                a.AlbumId,
                a.Title AS AlbumTitle,
                a.Slug AS AlbumSlug,
                a.ReleaseDate,
                m.Url AS CoverUrl
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN Track t ON t.EntityId = e.EntityId
            LEFT JOIN AlbumTrack at ON at.TrackId = t.TrackId
            LEFT JOIN Album a ON a.AlbumId = at.AlbumId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE c.PersonId = @PersonId
              AND et.Code = 'Track'
              AND t.IsDeleted = 0
              AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC, t.Title
            """;
        var results = await connection.QueryAsync<TrackContributionDto>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<TrackCreditRow>> GetPersonTrackCreditsAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                t.TrackId,
                cr.Name AS RoleName,
                i.Name AS InstrumentName
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN Track t ON t.EntityId = e.EntityId
            LEFT JOIN CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
            LEFT JOIN Instrument i ON i.InstrumentId = c.InstrumentId
            WHERE c.PersonId = @PersonId
              AND et.Code = 'Track'
              AND t.IsDeleted = 0
            """;
        var results = await connection.QueryAsync<TrackCreditRow>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyDictionary<int, IReadOnlyList<string>>> GetTrackGenresMapAsync(
        IDbConnection connection,
        int[] trackIds,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT tg.TrackId, g.Name AS GenreName
            FROM TrackGenre tg
            INNER JOIN Genre g ON g.GenreId = tg.GenreId
            WHERE tg.TrackId IN @TrackIds
            ORDER BY g.Name
            """;
        var rows = (await connection.QueryAsync<TrackGenreRow>(sql, new { TrackIds = trackIds })).AsList();

        var map = new Dictionary<int, IReadOnlyList<string>>();
        foreach (var group in rows.GroupBy(r => r.TrackId))
        {
            map[group.Key] = group.Select(r => r.GenreName).Distinct().ToList();
        }
        return map;
    }

    private static async Task<IReadOnlyList<RecordingSessionDto>> GetPersonSessionsAsync(
        IDbConnection connection,
        int[] albumIds,
        int[] trackIds,
        CancellationToken cancellationToken)
    {
        if (albumIds.Length == 0 && trackIds.Length == 0)
            return [];

        const string sql = """
            SELECT DISTINCT
                rs.RecordingSessionId,
                rs.Slug,
                rs.StartDate,
                rs.EndDate,
                st.Name AS SessionTypeName,
                l.Name AS LocationName,
                rs.Notes
            FROM RecordingSession rs
            LEFT JOIN SessionType st ON st.SessionTypeId = rs.SessionTypeId
            LEFT JOIN Location l ON l.LocationId = rs.LocationId
            WHERE rs.IsDeleted = 0
              AND (
                    rs.RecordingSessionId IN (SELECT rsa.RecordingSessionId FROM RecordingSessionAlbum rsa WHERE rsa.AlbumId IN @AlbumIds)
                 OR rs.RecordingSessionId IN (SELECT rst.RecordingSessionId FROM RecordingSessionTrack rst WHERE rst.TrackId IN @TrackIds)
              )
            ORDER BY rs.StartDate DESC
            """;
        var results = await connection.QueryAsync<RecordingSessionDto>(
            sql, new { AlbumIds = albumIds, TrackIds = trackIds });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<PerformanceEventDto>> GetPersonEventsAsync(
        IDbConnection connection,
        int[] albumIds,
        int[] trackIds,
        CancellationToken cancellationToken)
    {
        if (albumIds.Length == 0 && trackIds.Length == 0)
            return [];

        const string sql = """
            SELECT DISTINCT
                pe.PerformanceEventId,
                pe.Slug,
                pe.Date,
                et.Name AS EventTypeName,
                l.Name AS LocationName,
                pe.PerformanceNotes
            FROM PerformanceEvent pe
            LEFT JOIN EventType et ON et.EventTypeId = pe.EventTypeId
            LEFT JOIN Location l ON l.LocationId = pe.LocationId
            WHERE pe.IsDeleted = 0
              AND (
                    pe.PerformanceEventId IN (SELECT pea.PerformanceEventId FROM PerformanceEventAlbum pea WHERE pea.AlbumId IN @AlbumIds)
                 OR pe.PerformanceEventId IN (SELECT pet.PerformanceEventId FROM PerformanceEventTrack pet WHERE pet.TrackId IN @TrackIds)
              )
            ORDER BY pe.Date DESC
            """;
        var results = await connection.QueryAsync<PerformanceEventDto>(
            sql, new { AlbumIds = albumIds, TrackIds = trackIds });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AwardTimelineRow>> GetPersonAwardsAsync(
        IDbConnection connection,
        int[] albumIds,
        CancellationToken cancellationToken)
    {
        if (albumIds.Length == 0)
            return [];

        const string sql = """
            SELECT
                a.Name AS AwardName,
                a.Slug AS AwardSlug,
                aa.Year,
                art.Name AS Result,
                aa.Category,
                al.Title AS AlbumTitle,
                al.Slug AS AlbumSlug
            FROM AwardAssignment aa
            INNER JOIN Award a ON a.AwardId = aa.AwardId
            LEFT JOIN AwardResultType art ON art.AwardResultTypeId = aa.AwardResultTypeId
            INNER JOIN Album al ON al.EntityId = aa.EntityId AND aa.EntityTypeId = 1
            WHERE al.AlbumId IN @AlbumIds
              AND al.IsDeleted = 0
            ORDER BY aa.Year DESC
            """;
        var results = await connection.QueryAsync<AwardTimelineRow>(sql, new { AlbumIds = albumIds });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<ChartTimelineRow>> GetPersonChartsAsync(
        IDbConnection connection,
        int[] albumIds,
        CancellationToken cancellationToken)
    {
        if (albumIds.Length == 0)
            return [];

        const string sql = """
            SELECT
                c.Name AS ChartName,
                c.Slug AS ChartSlug,
                ce.Date,
                ce.Position,
                al.Title AS AlbumTitle,
                al.Slug AS AlbumSlug
            FROM ChartEntry ce
            INNER JOIN Chart c ON c.ChartId = ce.ChartId
            INNER JOIN Album al ON al.EntityId = ce.EntityId AND ce.EntityTypeId = 1
            WHERE al.AlbumId IN @AlbumIds
              AND al.IsDeleted = 0
            ORDER BY ce.Date DESC
            """;
        var results = await connection.QueryAsync<ChartTimelineRow>(sql, new { AlbumIds = albumIds });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<PublicationTimelineRow>> GetPersonPublicationsAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                p.Slug,
                p.Title,
                p.PublicationDate,
                pt.Name AS PublicationTypeName
            FROM Publication p
            LEFT JOIN PublicationType pt ON pt.PublicationTypeId = p.PublicationTypeId
            WHERE p.PersonId = @PersonId
              AND p.IsDeleted = 0
            ORDER BY p.PublicationDate DESC
            """;
        var results = await connection.QueryAsync<PublicationTimelineRow>(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static PersonDetailDto CreatePersonDetail(
        PersonDetailDto person,
        IReadOnlyDictionary<string, LocalizedFieldValues> localized,
        IReadOnlyList<MediaDto> media,
        IReadOnlyList<EntityLinkDto> links,
        IReadOnlyList<AliasDto> aliases,
        IReadOnlyList<TagDto> tags,
        IReadOnlyList<CitationDto> citations,
        IReadOnlyList<NamedLinkDto> instruments,
        IReadOnlyList<NamedLinkDto> roles,
        IReadOnlyList<PersonAlbumDto> albums,
        IReadOnlyList<TrackContributionDto> contributions,
        IReadOnlyDictionary<int, IReadOnlyList<string>> trackGenres,
        IReadOnlyList<RecordingSessionDto> sessions,
        IReadOnlyList<PerformanceEventDto> events,
        IReadOnlyList<AwardTimelineRow> awards,
        IReadOnlyList<ChartTimelineRow> chartRows,
        IReadOnlyList<PublicationTimelineRow> publications)
    {
        return new PersonDetailDto
        {
            PersonId = person.PersonId,
            EntityId = person.EntityId,
            Slug = person.Slug,
            FullName = LocalizationHelper.Pick(localized, "Name", person.FullName),
            OriginalName = LocalizationHelper.Pick(localized, "OriginalName", person.OriginalName),
            EnglishName = LocalizationHelper.Pick(localized, "EnglishName", person.EnglishName),
            PersonKind = person.PersonKind,
            Nationality = person.Nationality,
            BirthDate = person.BirthDate,
            BirthDatePrecision = person.BirthDatePrecision,
            DeathDate = person.DeathDate,
            DeathDatePrecision = person.DeathDatePrecision,
            Biography = LocalizationHelper.Pick(localized, "Biography", person.Biography),
            ImageUrl = person.ImageUrl,
            Media = media,
            Links = links,
            Aliases = aliases,
            Tags = tags,
            Citations = citations,
            Instruments = instruments,
            Roles = roles,
            Albums = albums,
            TrackContributions = contributions,
            Timeline = BuildTimeline(albums, contributions, sessions, events, awards, chartRows, publications),
            AlbumCategories = albums
                .Select(a => a.CategoryName)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(n => n!)
                .Distinct()
                .OrderBy(n => n)
                .ToList(),
            AlbumYears = albums
                .Select(a => a.ReleaseDate?.Year)
                .Where(y => y.HasValue)
                .Select(y => y!.Value)
                .Distinct()
                .OrderByDescending(y => y)
                .ToList(),
            AlbumRoles = albums.SelectMany(a => a.Roles).Distinct().OrderBy(r => r).ToList(),
            AlbumInstruments = albums.SelectMany(a => a.Instruments).Distinct().OrderBy(i => i).ToList(),
            ContributionRoles = contributions.SelectMany(c => c.Roles).Distinct().OrderBy(r => r).ToList(),
            ContributionInstruments = contributions.SelectMany(c => c.Instruments).Distinct().OrderBy(i => i).ToList(),
            ContributionAlbums = contributions
                .Where(c => !string.IsNullOrWhiteSpace(c.AlbumTitle))
                .Select(c => c.AlbumTitle!)
                .Distinct()
                .OrderBy(t => t)
                .ToList(),
            ContributionGenres = contributions.SelectMany(c => c.Genres).Distinct().OrderBy(g => g).ToList()
        };
    }

    private static IReadOnlyList<TimelineEntryDto> BuildTimeline(
        IReadOnlyList<PersonAlbumDto> albums,
        IReadOnlyList<TrackContributionDto> contributions,
        IReadOnlyList<RecordingSessionDto> sessions,
        IReadOnlyList<PerformanceEventDto> events,
        IReadOnlyList<AwardTimelineRow> awards,
        IReadOnlyList<ChartTimelineRow> chartRows,
        IReadOnlyList<PublicationTimelineRow> publications)
    {
        var timeline = new List<TimelineEntryDto>();

        foreach (var album in albums)
        {
            timeline.Add(new TimelineEntryDto
            {
                Type = "AlbumRelease",
                Title = album.Title,
                Date = album.ReleaseDate?.ToDateTime(TimeOnly.MinValue),
                Slug = album.Slug,
                RouteName = "albums",
                Detail = album.CategoryName
            });
        }

        foreach (var contribution in contributions)
        {
            timeline.Add(new TimelineEntryDto
            {
                Type = "TrackRelease",
                Title = contribution.Title,
                Date = contribution.ReleaseDate?.ToDateTime(TimeOnly.MinValue),
                Slug = contribution.Slug,
                RouteName = "tracks",
                Detail = contribution.AlbumTitle
            });
        }

        foreach (var session in sessions)
        {
            timeline.Add(new TimelineEntryDto
            {
                Type = "RecordingSession",
                Title = session.SessionTypeName ?? "Recording Session",
                Date = session.StartDate?.ToDateTime(TimeOnly.MinValue),
                Slug = session.Slug,
                RouteName = "sessions",
                Detail = session.LocationName
            });
        }

        foreach (var evt in events)
        {
            timeline.Add(new TimelineEntryDto
            {
                Type = "PerformanceEvent",
                Title = evt.EventTypeName ?? "Performance Event",
                Date = evt.Date,
                Slug = evt.Slug,
                RouteName = "events",
                Detail = evt.LocationName
            });
        }

        foreach (var award in awards)
        {
            timeline.Add(new TimelineEntryDto
            {
                Type = "Award",
                Title = award.AwardName,
                Year = award.Year,
                Slug = award.AwardSlug,
                RouteName = "awards",
                Detail = BuildAwardDetail(award)
            });
        }

        foreach (var chart in chartRows)
        {
            timeline.Add(new TimelineEntryDto
            {
                Type = "ChartEntry",
                Title = chart.ChartName,
                Date = chart.Date?.ToDateTime(TimeOnly.MinValue),
                Slug = chart.ChartSlug,
                RouteName = "charts",
                Detail = BuildChartDetail(chart)
            });
        }

        foreach (var publication in publications)
        {
            // Publications have no public route yet — rendered without a link.
            timeline.Add(new TimelineEntryDto
            {
                Type = "Publication",
                Title = publication.Title,
                Date = publication.PublicationDate?.ToDateTime(TimeOnly.MinValue),
                Detail = publication.PublicationTypeName
            });
        }

        timeline.Sort((x, y) => DateTime.Compare(TimelineSortKey(y), TimelineSortKey(x)));
        return timeline;
    }

    private static string? BuildAwardDetail(AwardTimelineRow award)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(award.Result))
            parts.Add(award.Result);
        if (!string.IsNullOrWhiteSpace(award.Category))
            parts.Add(award.Category);
        var detail = string.Join(" · ", parts);
        if (!string.IsNullOrWhiteSpace(award.AlbumTitle))
            detail = detail.Length == 0 ? award.AlbumTitle : $"{detail} — {award.AlbumTitle}";
        return detail.Length == 0 ? null : detail;
    }

    private static string? BuildChartDetail(ChartTimelineRow chart)
    {
        var detail = $"#{chart.Position}";
        if (!string.IsNullOrWhiteSpace(chart.AlbumTitle))
            detail = $"{detail} — {chart.AlbumTitle}";
        return detail;
    }

    private static DateTime TimelineSortKey(TimelineEntryDto entry)
    {
        if (entry.Date.HasValue)
            return entry.Date.Value;
        if (entry.Year.HasValue)
            return new DateTime(entry.Year.Value, 1, 1);
        return DateTime.MinValue;
    }

    // ────────────────────────────────────────────────────────────────
    // Classic entity section queries
    // ────────────────────────────────────────────────────────────────

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

    // ────────────────────────────────────────────────────────────────
    // Private row types (Dapper projection)
    // ────────────────────────────────────────────────────────────────

    private sealed class AlbumCreditRow
    {
        public int AlbumId { get; init; }
        public string? RoleName { get; init; }
        public string? InstrumentName { get; init; }
    }

    private sealed class TrackCreditRow
    {
        public int TrackId { get; init; }
        public string? RoleName { get; init; }
        public string? InstrumentName { get; init; }
    }

    private sealed class TrackGenreRow
    {
        public int TrackId { get; init; }
        public string GenreName { get; init; } = "";
    }

    private sealed class AwardTimelineRow
    {
        public string AwardName { get; init; } = "";
        public string AwardSlug { get; init; } = "";
        public int? Year { get; init; }
        public string? Result { get; init; }
        public string? Category { get; init; }
        public string? AlbumTitle { get; init; }
        public string? AlbumSlug { get; init; }
    }

    private sealed class ChartTimelineRow
    {
        public string ChartName { get; init; } = "";
        public string ChartSlug { get; init; } = "";
        public DateOnly? Date { get; init; }
        public int Position { get; init; }
        public string? AlbumTitle { get; init; }
        public string? AlbumSlug { get; init; }
    }

    private sealed class PublicationTimelineRow
    {
        public string Slug { get; init; } = "";
        public string Title { get; init; } = "";
        public DateOnly? PublicationDate { get; init; }
        public string? PublicationTypeName { get; init; }
    }
}
