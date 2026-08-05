using System.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for performance event listing and detail pages.
/// Routes: /{culture}/events
/// Spec reference: 9.15
/// </summary>
[Route("{culture:regex(^(fa)$)}/events")]
public sealed class EventsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<EventsController> _logger;

    public EventsController(IDbConnection db, ILogger<EventsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 24;
        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM PerformanceEvent pe WHERE pe.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                pe.PerformanceEventId,
                pe.Slug,
                et.Name AS EventTypeName,
                pe.Date,
                l.Name AS VenueName,
                pe.PerformanceNotes AS PerformanceNotesPreview
            FROM PerformanceEvent pe
            LEFT JOIN EventType et ON pe.EventTypeId = et.EventTypeId
            LEFT JOIN Location l ON pe.LocationId = l.LocationId
            WHERE pe.IsDeleted = 0
            ORDER BY pe.Date DESC, pe.PerformanceEventId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await _db.QueryAsync<EventListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();

        // Truncate PerformanceNotesPreview to 200 chars (LEFT() is SQL Server-specific)
        foreach (var item in items)
        {
            if (item.PerformanceNotesPreview?.Length > 200)
            {
                item.PerformanceNotesPreview = item.PerformanceNotesPreview[..200] + "...";
            }
        }

        var result = MusicEncyclopedia.Core.DTOs.PagedResult<EventListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new EventListViewModel { Items = result, Culture = culture };

        ViewData["Title"] = "Performance Events";
        ViewData["MetaDescription"] = "Browse performance events — concert details, venue, date, and more.";

        return View(viewModel);
    }

    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        var eventSql = @"
            SELECT 
                pe.PerformanceEventId,
                pe.Slug,
                et.Name AS EventTypeName,
                pe.Date,
                pe.AudienceInfo,
                pe.PerformanceNotes,
                pe.ImprovisationNotes,
                pe.LocationId,
                l.Name AS VenueName,
                l.Slug AS VenueSlug
            FROM PerformanceEvent pe
            LEFT JOIN EventType et ON pe.EventTypeId = et.EventTypeId
            LEFT JOIN Location l ON pe.LocationId = l.LocationId
            WHERE pe.Slug = @Slug AND pe.IsDeleted = 0";

        var evt = await _db.QuerySingleOrDefaultAsync<EventDetailDto>(eventSql, new { Slug = slug });

        if (evt is null)
        {
            _logger.LogWarning("Event not found: slug={Slug}", slug);
            return NotFound();
        }

        // Albums performed
        var albumsSql = @"
            SELECT a.AlbumId, a.Title, a.Slug, '' AS CoverUrl
            FROM PerformanceEventAlbum pea
            INNER JOIN Album a ON pea.AlbumId = a.AlbumId
            WHERE pea.PerformanceEventId = @Id AND a.IsDeleted = 0";
        evt = evt with { Albums = (await _db.QueryAsync<EventAlbumDto>(albumsSql, new { Id = evt.PerformanceEventId })).ToList() };

        // Tracks performed
        var tracksSql = @"
            SELECT t.TrackId, t.Title, t.Slug, t.DurationSeconds
            FROM PerformanceEventTrack pet
            INNER JOIN Track t ON pet.TrackId = t.TrackId
            WHERE pet.PerformanceEventId = @Id AND t.IsDeleted = 0";
        evt = evt with { Tracks = (await _db.QueryAsync<EventTrackDto>(tracksSql, new { Id = evt.PerformanceEventId })).ToList() };

        // Media
        var mediaSql = @"
            SELECT m.MediaId, m.FileName, m.FilePath, m.Url, 
                   m.ThumbnailUrl150, m.ThumbnailUrl300, m.ThumbnailUrl600,
                   m.MimeType, ma.IsPrimary, ma.DisplayOrder,
                   mrt.Name AS MediaRoleName
            FROM MediaAssignment ma
            INNER JOIN Media m ON ma.MediaId = m.MediaId
            LEFT JOIN MediaRoleType mrt ON ma.MediaRoleTypeId = mrt.MediaRoleTypeId
            INNER JOIN PerformanceEvent pe ON pe.EntityId = ma.EntityId AND ma.EntityTypeId = 12
            WHERE pe.Slug = @Slug AND m.IsDeleted = 0
            ORDER BY ma.DisplayOrder";
        var media = (await _db.QueryAsync<MediaDto>(mediaSql, new { Slug = slug })).ToList();
        evt = evt with { Media = media };

        // Citations
        var citationsSql = @"
            SELECT c.CitationId, c.EntityTypeId, c.EntityId, c.FieldName, c.Quote, c.PageNumber, c.Url AS CitationUrl,
                   c.AccessedDate, s.Title AS SourceTitle, s.Slug AS SourceSlug
            FROM Citation c
            LEFT JOIN Source s ON c.SourceId = s.SourceId
            INNER JOIN PerformanceEvent pe ON pe.EntityId = c.EntityId AND c.EntityTypeId = 12
            WHERE pe.Slug = @Slug";
        var citations = (await _db.QueryAsync<CitationDto>(citationsSql, new { Slug = slug })).ToList();
        evt = evt with { Citations = citations };

        var viewModel = new EventDetailViewModel { Event = evt, Culture = culture };

        ViewData["Title"] = $"Performance Event: {evt.Slug}";
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(evt.PerformanceNotes)
            ? (evt.PerformanceNotes.Length > 200 ? evt.PerformanceNotes[..200] + "..." : evt.PerformanceNotes)
            : $"Performance event: {evt.Slug}";
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/events/{slug}";

        return View(viewModel);
    }
}
