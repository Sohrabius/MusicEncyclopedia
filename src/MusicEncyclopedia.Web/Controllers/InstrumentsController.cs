using System.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for instrument listing and detail pages.
/// Routes: /{culture}/instruments and /{culture}/instruments/{slug}
/// Spec references: 8.1 (routes), 9.10 (detail page)
/// </summary>
[Route("{culture:regex(^(fa)$)}/instruments")]
public sealed class InstrumentsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<InstrumentsController> _logger;

    public InstrumentsController(
        IDbConnection db,
        ILogger<InstrumentsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Instrument listing page (9.10).
    /// Shows all instruments as a simple paginated list.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Instrument list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        const int pageSize = 24;

        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM Instrument WHERE IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var itemsSql = q is null
            ? "SELECT InstrumentId, Name, Slug FROM Instrument WHERE IsDeleted = 0 ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY"
            : "SELECT InstrumentId, Name, Slug FROM Instrument WHERE IsDeleted = 0 AND (Name LIKE @Q OR Description LIKE @Q) ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        // Use named mapping since column is InstrumentId not Id
        var items = (await _db.QueryAsync<(int InstrumentId, string Name, string Slug)>(
            itemsSql,
            new { Offset = offset, PageSize = pageSize, Q = q != null ? $"%{q}%" : null }))
            .Select(x => new NamedLinkDto { Name = x.Name, Slug = x.Slug })
            .ToList();

        var viewModel = new InstrumentListViewModel
        {
            Items = PagedResult<NamedLinkDto>.Create(
                items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "Instruments";
        ViewData["MetaDescription"] = "Browse all musical instruments — from strings and winds to percussion and electronics.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Instrument detail page (9.10).
    /// Displays full instrument information including family, country of origin,
    /// historical notes, musicians, tracks, track credits, media, and references.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Instrument detail requested: culture={Culture}, slug={Slug}", culture, slug);

        // 1. Main instrument with family and country
        var instrument = await _db.QueryFirstOrDefaultAsync<(
            int InstrumentId,
            string Name,
            string Slug,
            string? Description,
            int? InstrumentFamilyId,
            int? CountryId,
            string? HistoricalNotes)>(
            "SELECT InstrumentId, Name, Slug, Description, InstrumentFamilyId, CountryId, HistoricalNotes " +
            "FROM Instrument WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (instrument == default)
        {
            _logger.LogWarning("Instrument not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        var viewModel = new InstrumentDetailViewModel
        {
            InstrumentId = instrument.InstrumentId,
            Name = instrument.Name,
            Slug = instrument.Slug,
            Description = instrument.Description,
            HistoricalNotes = instrument.HistoricalNotes,
            Culture = culture
        };

        // 2. Instrument family
        if (instrument.InstrumentFamilyId.HasValue)
        {
            var family = await _db.ExecuteScalarAsync<string>(
                "SELECT Name FROM InstrumentFamily WHERE InstrumentFamilyId = @Id",
                new { Id = instrument.InstrumentFamilyId.Value });

            viewModel.FamilyName = family;
        }

        // 3. Country of origin
        if (instrument.CountryId.HasValue)
        {
            var country = await _db.ExecuteScalarAsync<string>(
                "SELECT Name FROM Country WHERE CountryId = @Id",
                new { Id = instrument.CountryId.Value });

            viewModel.CountryOfOrigin = country;
        }

        // 4. Musicians who play this instrument
        var musicians = await _db.QueryAsync<(int PersonId, string Name, string Slug, string? Notes)>(
            @"SELECT p.PersonId, p.FullName AS Name, p.Slug, mi.Notes
              FROM MusicianInstrument mi
              INNER JOIN Person p ON mi.PersonId = p.PersonId
              WHERE mi.InstrumentId = @InstrumentId AND p.IsDeleted = 0
              ORDER BY p.FullName",
            new { InstrumentId = instrument.InstrumentId });

        viewModel.Musicians = musicians.Select(m => new InstrumentMusicianDto
        {
            PersonId = m.PersonId,
            Name = m.Name,
            Slug = m.Slug,
            Notes = m.Notes
        }).ToList();

        // 5. Tracks using this instrument (via TrackInstrument)
        var tracks = await _db.QueryAsync<(int TrackId, string Title, string Slug, int? DurationSeconds)>(
            @"SELECT t.TrackId, t.Title, t.Slug, t.DurationSeconds
              FROM Track t
              INNER JOIN TrackInstrument ti ON t.TrackId = ti.TrackId
              WHERE ti.InstrumentId = @InstrumentId AND t.IsDeleted = 0
              ORDER BY t.Title",
            new { InstrumentId = instrument.InstrumentId });

        viewModel.Tracks = tracks.Select(t => new InstrumentTrackDto
        {
            TrackId = t.TrackId,
            Title = t.Title,
            Slug = t.Slug,
            DurationSeconds = t.DurationSeconds
        }).ToList();

        // 6. Track credits involving this instrument (Credits with InstrumentId)
        var entityTypeIds = await GetEntityTypeIdsAsync(_db, ["Instrument", "Track"]);

        var trackCredits = await _db.QueryAsync<(int CreditId, string TrackTitle, string TrackSlug, string? PersonName, string? PersonSlug)>(
            @"SELECT c.CreditId, t2.Title AS TrackTitle, t2.Slug AS TrackSlug,
                     p.FullName AS PersonName, p.Slug AS PersonSlug
              FROM Credit c
              INNER JOIN Track t2 ON c.EntityTypeId = @TrackTypeId AND c.EntityId = t2.TrackId
              LEFT JOIN Person p ON c.PersonId = p.PersonId
              WHERE c.InstrumentId = @InstrumentId AND t2.IsDeleted = 0 AND (p.IsDeleted = 0 OR p.PersonId IS NULL)
              ORDER BY t2.Title",
            new
            {
                InstrumentId = instrument.InstrumentId,
                TrackTypeId = entityTypeIds.GetValueOrDefault("Track", 0)
            });

        viewModel.TrackCredits = trackCredits.Select(tc => new InstrumentTrackCreditDto
        {
            CreditId = tc.CreditId,
            TrackTitle = tc.TrackTitle,
            TrackSlug = tc.TrackSlug,
            PersonName = tc.PersonName,
            PersonSlug = tc.PersonSlug
        }).ToList();

        // 7. Media attached via Entity -> MediaAssignment
        var instrumentEntityId = await _db.ExecuteScalarAsync<int?>(
            "SELECT EntityId FROM Instrument WHERE InstrumentId = @InstrumentId",
            new { InstrumentId = instrument.InstrumentId });

        if (instrumentEntityId.HasValue)
        {
            var media = await _db.QueryAsync<(int MediaId, string Url, string? ThumbnailUrl, string? Description, string? MediaTypeName)>(
                @"SELECT m.MediaId,
                         COALESCE(m.Url, m.FilePath) AS Url,
                         m.ThumbnailUrl300 AS ThumbnailUrl,
                         ma.Description,
                         mt.Name AS MediaTypeName
                  FROM MediaAssignment ma
                  INNER JOIN Media m ON ma.MediaId = m.MediaId
                  LEFT JOIN MediaType mt ON m.MediaTypeId = mt.MediaTypeId
                  WHERE ma.EntityTypeId = @EntityTypeId AND ma.EntityId = @EntityId AND m.IsDeleted = 0
                  ORDER BY ma.DisplayOrder",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Instrument", 0), EntityId = instrumentEntityId.Value });

            viewModel.Media = media.Select(m => new InstrumentMediaDto
            {
                MediaId = m.MediaId,
                Url = m.Url,
                ThumbnailUrl = m.ThumbnailUrl,
                Description = m.Description,
                MediaType = m.MediaTypeName
            }).ToList();

            // 8. Citations
            var citations = await _db.QueryAsync<(int CitationId, string? Quote, string? SourceName, string? SourceSlug, string? PageNumber, string? Url)>(
                @"SELECT c.CitationId, c.Quote, s.Title AS SourceName, s.Slug AS SourceSlug, c.PageNumber, c.Url
                  FROM Citation c
                  LEFT JOIN [Source] s ON c.SourceId = s.SourceId
                  WHERE c.EntityTypeId = @EntityTypeId AND c.EntityId = @EntityId
                  ORDER BY c.CitationId",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Instrument", 0), EntityId = instrumentEntityId.Value });

            viewModel.Citations = citations.Select(c => new InstrumentCitationDto
            {
                CitationId = c.CitationId,
                Quote = c.Quote,
                SourceName = c.SourceName,
                SourceSlug = c.SourceSlug,
                PageNumber = c.PageNumber,
                Url = c.Url
            }).ToList();
        }

        ViewData["Title"] = instrument.Name;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(instrument.Description)
            ? (instrument.Description.Length > 200 ? instrument.Description[..200] + "..." : instrument.Description)
            : $"Instrument: {instrument.Name}";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/instruments/{slug}";

        return View(viewModel);
    }

    /// <summary>
    /// Returns a dictionary mapping entity type codes to their integer IDs.
    /// </summary>
    private static async Task<Dictionary<string, int>> GetEntityTypeIdsAsync(
        IDbConnection connection,
        string[] codes)
    {
        var rows = await connection.QueryAsync<(int EntityTypeId, string Code)>(
            "SELECT EntityTypeId, Code FROM EntityType WHERE Code IN @Codes",
            new { Codes = codes });

        return rows.ToDictionary(r => r.Code, r => r.EntityTypeId);
    }
}
