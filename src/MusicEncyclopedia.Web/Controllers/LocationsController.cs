using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for location listing and detail pages.
/// Routes: /{culture}/locations
/// Spec reference: 9.16
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/locations")]
public sealed class LocationsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(IDbConnection db, ILogger<LocationsController> logger)
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

        var countSql = "SELECT COUNT(*) FROM Location l WHERE l.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                l.LocationId,
                l.Name,
                l.Slug,
                lt.Name AS LocationTypeName,
                pl.Name AS ParentLocationName,
                c.Name AS CountryName
            FROM Location l
            LEFT JOIN LocationType lt ON l.LocationTypeId = lt.LocationTypeId
            LEFT JOIN Location pl ON l.ParentLocationId = pl.LocationId
            LEFT JOIN Country c ON l.CountryId = c.CountryId
            WHERE l.IsDeleted = 0
            ORDER BY l.Name
            " + SqlDialect.Pagination(SqlDialect.IsSqliteConnection(_db));

        var items = (await _db.QueryAsync<LocationListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();
        var result = MusicEncyclopedia.Core.DTOs.PagedResult<LocationListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new LocationListViewModel { Items = result, Culture = culture };

        ViewData["Title"] = "Locations";
        ViewData["MetaDescription"] = "Browse locations — studios, venues, cities, and more.";

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
        var locationSql = @"
            SELECT 
                l.LocationId,
                l.Name,
                l.Slug,
                lt.Name AS LocationTypeName,
                c.Name AS CountryName,
                l.Latitude,
                l.Longitude,
                l.ParentLocationId,
                pl.Name AS ParentLocationName,
                pl.Slug AS ParentLocationSlug
            FROM Location l
            LEFT JOIN LocationType lt ON l.LocationTypeId = lt.LocationTypeId
            LEFT JOIN Location pl ON l.ParentLocationId = pl.LocationId
            LEFT JOIN Country c ON l.CountryId = c.CountryId
            WHERE l.Slug = @Slug AND l.IsDeleted = 0";

        var location = await _db.QuerySingleOrDefaultAsync<LocationDetailDto>(locationSql, new { Slug = slug });

        if (location is null)
        {
            _logger.LogWarning("Location not found: slug={Slug}", slug);
            return NotFound();
        }

        // Child locations
        var childrenSql = @"
            SELECT l.LocationId, l.Name, l.Slug, lt.Name AS LocationTypeName
            FROM Location l
            LEFT JOIN LocationType lt ON l.LocationTypeId = lt.LocationTypeId
            WHERE l.ParentLocationId = @Id AND l.IsDeleted = 0
            ORDER BY l.Name";
        location = location with { Children = (await _db.QueryAsync<LocationChildDto>(childrenSql, new { Id = location.LocationId })).ToList() };

        // Sessions
        var sessionsSql = @"
            SELECT rs.RecordingSessionId, rs.Slug, st.Name AS SessionTypeName, rs.StartDate
            FROM RecordingSession rs
            LEFT JOIN SessionType st ON rs.SessionTypeId = st.SessionTypeId
            WHERE rs.LocationId = @Id AND rs.IsDeleted = 0
            ORDER BY rs.StartDate DESC";
        location = location with { Sessions = (await _db.QueryAsync<LocationSessionDto>(sessionsSql, new { Id = location.LocationId })).ToList() };

        // Events
        var eventsSql = @"
            SELECT pe.PerformanceEventId, pe.Slug, et.Name AS EventTypeName, pe.Date
            FROM PerformanceEvent pe
            LEFT JOIN EventType et ON pe.EventTypeId = et.EventTypeId
            WHERE pe.LocationId = @Id AND pe.IsDeleted = 0
            ORDER BY pe.Date DESC";
        location = location with { Events = (await _db.QueryAsync<LocationEventDto>(eventsSql, new { Id = location.LocationId })).ToList() };

        // People born here
        var bornSql = @"
            SELECT p.PersonId, p.FullName, p.Slug
            FROM Person p
            WHERE p.BirthLocationId = @Id AND p.IsDeleted = 0
            ORDER BY p.FullName";
        location = location with { PeopleBornHere = (await _db.QueryAsync<LocationPersonDto>(bornSql, new { Id = location.LocationId })).ToList() };

        // People died here
        var diedSql = @"
            SELECT p.PersonId, p.FullName, p.Slug
            FROM Person p
            WHERE p.DeathLocationId = @Id AND p.IsDeleted = 0
            ORDER BY p.FullName";
        location = location with { PeopleDiedHere = (await _db.QueryAsync<LocationPersonDto>(diedSql, new { Id = location.LocationId })).ToList() };

        // Media
        var mediaSql = @"
            SELECT m.MediaId, m.FileName, m.FilePath, m.Url, 
                   m.ThumbnailUrl150, m.ThumbnailUrl300, m.ThumbnailUrl600,
                   m.MimeType, ma.IsPrimary, ma.DisplayOrder,
                   mrt.Name AS MediaRoleName
            FROM MediaAssignment ma
            INNER JOIN Media m ON ma.MediaId = m.MediaId
            LEFT JOIN MediaRoleType mrt ON ma.MediaRoleTypeId = mrt.MediaRoleTypeId
            INNER JOIN Location l ON l.EntityId = ma.EntityId AND ma.EntityTypeId = 13
            WHERE l.Slug = @Slug AND m.IsDeleted = 0
            ORDER BY ma.DisplayOrder";
        var media = (await _db.QueryAsync<MediaDto>(mediaSql, new { Slug = slug })).ToList();
        location = location with { Media = media };

        var viewModel = new LocationDetailViewModel { Location = location, Culture = culture };

        ViewData["Title"] = location.Name;
        ViewData["MetaDescription"] = $"Location: {location.Name}" + (!string.IsNullOrWhiteSpace(location.CountryName) ? $", {location.CountryName}" : "");
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/locations/{slug}";

        return View(viewModel);
    }
}
