using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for recording session listing and detail pages.
/// Routes: /{culture}/sessions
/// Spec reference: 9.14
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/sessions")]
public sealed class SessionsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(IDbConnection db, ILogger<SessionsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Session listing page.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 24;

        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM RecordingSession rs WHERE rs.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                rs.RecordingSessionId,
                rs.Slug,
                st.Name AS SessionTypeName,
                rs.StartDate,
                rs.EndDate,
                l.Name AS LocationName,
                rs.Notes AS NotesPreview
            FROM RecordingSession rs
            LEFT JOIN SessionType st ON rs.SessionTypeId = st.SessionTypeId
            LEFT JOIN Location l ON rs.LocationId = l.LocationId
            WHERE rs.IsDeleted = 0
            ORDER BY rs.StartDate DESC, rs.RecordingSessionId DESC
            " + SqlDialect.Pagination();

        var items = (await _db.QueryAsync<SessionListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();

        // Truncate NotesPreview to 200 chars (LEFT() is SQL Server-specific)
        foreach (var item in items)
        {
            if (item.NotesPreview?.Length > 200)
            {
                item.NotesPreview = item.NotesPreview[..200] + "...";
            }
        }

        var result = MusicEncyclopedia.Core.DTOs.PagedResult<SessionListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new SessionListViewModel
        {
            Items = result,
            Culture = culture
        };

        ViewData["Title"] = "Recording Sessions";
        ViewData["MetaDescription"] = "Browse recording sessions — session type, date, location, and more.";

        return View(viewModel);
    }

    /// <summary>
    /// Session detail page.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        var sessionSql = @"
            SELECT 
                rs.RecordingSessionId,
                rs.Slug,
                st.Name AS SessionTypeName,
                rs.StartDate,
                rs.EndDate,
                rs.DatePrecision,
                rs.Notes,
                rs.LocationId,
                l.Name AS LocationName,
                l.Slug AS LocationSlug
            FROM RecordingSession rs
            LEFT JOIN SessionType st ON rs.SessionTypeId = st.SessionTypeId
            LEFT JOIN Location l ON rs.LocationId = l.LocationId
            WHERE rs.Slug = @Slug AND rs.IsDeleted = 0";

        var session = await _db.QuerySingleOrDefaultAsync<SessionDetailDto>(sessionSql, new { Slug = slug });

        if (session is null)
        {
            _logger.LogWarning("Session not found: slug={Slug}", slug);
            return NotFound();
        }

        // Load albums
        var albumsSql = @"
            SELECT a.AlbumId, a.Title, a.Slug, '' AS CoverUrl
            FROM RecordingSessionAlbum rsa
            INNER JOIN Album a ON rsa.AlbumId = a.AlbumId
            WHERE rsa.RecordingSessionId = @Id AND a.IsDeleted = 0";
        session = session with { Albums = (await _db.QueryAsync<SessionAlbumDto>(albumsSql, new { Id = session.RecordingSessionId })).ToList() };

        // Load tracks
        var tracksSql = @"
            SELECT t.TrackId, t.Title, t.Slug, t.DurationSeconds
            FROM RecordingSessionTrack rst
            INNER JOIN Track t ON rst.TrackId = t.TrackId
            WHERE rst.RecordingSessionId = @Id AND t.IsDeleted = 0";
        session = session with { Tracks = (await _db.QueryAsync<SessionTrackDto>(tracksSql, new { Id = session.RecordingSessionId })).ToList() };

        // Load media
        var mediaSql = @"
            SELECT m.MediaId, m.FileName, m.FilePath, m.Url, 
                   m.ThumbnailUrl150, m.ThumbnailUrl300, m.ThumbnailUrl600,
                   m.MimeType, ma.IsPrimary, ma.DisplayOrder,
                   mrt.Name AS MediaRoleName
            FROM MediaAssignment ma
            INNER JOIN Media m ON ma.MediaId = m.MediaId
            LEFT JOIN MediaRoleType mrt ON ma.MediaRoleTypeId = mrt.MediaRoleTypeId
            INNER JOIN RecordingSession rs ON rs.EntityId = ma.EntityId AND ma.EntityTypeId = 11
            WHERE rs.Slug = @Slug AND m.IsDeleted = 0
            ORDER BY ma.DisplayOrder";
        // Note: EntityTypeId for RecordingSession needs verification; using 11 as placeholder
        var media = (await _db.QueryAsync<MediaDto>(mediaSql, new { Slug = slug })).ToList();
        session = session with { Media = media };

        // Load citations
        var citationsSql = @"
            SELECT c.CitationId, c.EntityTypeId, c.EntityId, c.FieldName, c.Quote, c.PageNumber, c.Url AS CitationUrl,
                   c.AccessedDate, s.Title AS SourceTitle, s.Slug AS SourceSlug
            FROM Citation c
            LEFT JOIN Source s ON c.SourceId = s.SourceId
            INNER JOIN RecordingSession rs ON rs.EntityId = c.EntityId AND c.EntityTypeId = 11
            WHERE rs.Slug = @Slug";
        var citations = (await _db.QueryAsync<CitationDto>(citationsSql, new { Slug = slug })).ToList();
        session = session with { Citations = citations };

        var viewModel = new SessionDetailViewModel
        {
            Session = session,
            Culture = culture
        };

        ViewData["Title"] = $"Recording Session: {session.Slug}";
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(session.Notes)
            ? (session.Notes.Length > 200 ? session.Notes[..200] + "..." : session.Notes)
            : $"Recording session: {session.Slug}";
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/sessions/{slug}";

        return View(viewModel);
    }
}
