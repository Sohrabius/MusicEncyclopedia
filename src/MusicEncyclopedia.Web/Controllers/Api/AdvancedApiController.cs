using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for advanced resources:
/// Sessions, Events, Locations, Awards, Certifications, Charts.
/// Spec 11.1 — Sessions, Events, Locations, Awards, Certifications, Charts endpoints.
/// Uses Dapper directly since no dedicated services exist for these entities.
/// </summary>
public sealed class AdvancedApiController : BaseApiController
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdvancedApiController> _logger;

    public AdvancedApiController(
        IConfiguration configuration,
        ILogger<AdvancedApiController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  Recording Sessions
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/sessions — paginated session listing.
    /// </summary>
    [HttpGet("sessions")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetSessions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM RecordingSession WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                rs.RecordingSessionId,
                rs.Slug,
                rs.Title,
                rs.StartDate,
                rs.EndDate,
                rs.Notes,
                st.Name AS SessionTypeName,
                l.Name AS LocationName,
                l.Slug AS LocationSlug
            FROM RecordingSession rs
            LEFT JOIN SessionType st ON st.SessionTypeId = rs.SessionTypeId
            LEFT JOIN Location l ON l.LocationId = rs.LocationId
            WHERE rs.IsDeleted = 0
            ORDER BY rs.StartDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/sessions/{slug} — session detail.
    /// </summary>
    [HttpGet("sessions/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetSessionBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                rs.RecordingSessionId,
                rs.EntityId,
                rs.Slug,
                rs.Title,
                rs.StartDate,
                rs.EndDate,
                rs.Notes,
                st.Name AS SessionTypeName,
                l.Name AS LocationName,
                l.Slug AS LocationSlug
            FROM RecordingSession rs
            LEFT JOIN SessionType st ON st.SessionTypeId = rs.SessionTypeId
            LEFT JOIN Location l ON l.LocationId = rs.LocationId
            WHERE rs.Slug = @Slug AND rs.IsDeleted = 0";

        var session = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (session is null)
            return NotFoundResult($"Session with slug '{slug}' not found.");

        return OkResult(session);
    }

    // ──────────────────────────────────────────────
    //  Performance Events
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/events — paginated event listing.
    /// </summary>
    [HttpGet("events")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetEvents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM PerformanceEvent WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                pe.PerformanceEventId,
                pe.Slug,
                pe.Title,
                pe.EventDate,
                pe.StartTime,
                pe.EndTime,
                pe.AudienceInformation,
                pe.PerformanceNotes,
                pe.ImprovisationNotes,
                et.Name AS EventTypeName,
                l.Name AS LocationName,
                l.Slug AS LocationSlug
            FROM PerformanceEvent pe
            LEFT JOIN EventType et ON et.EventTypeId = pe.EventTypeId
            LEFT JOIN Location l ON l.LocationId = pe.LocationId
            WHERE pe.IsDeleted = 0
            ORDER BY pe.EventDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/events/{slug} — event detail.
    /// </summary>
    [HttpGet("events/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetEventBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                pe.PerformanceEventId,
                pe.EntityId,
                pe.Slug,
                pe.Title,
                pe.EventDate,
                pe.StartTime,
                pe.EndTime,
                pe.AudienceInformation,
                pe.PerformanceNotes,
                pe.ImprovisationNotes,
                et.Name AS EventTypeName,
                l.Name AS LocationName,
                l.Slug AS LocationSlug
            FROM PerformanceEvent pe
            LEFT JOIN EventType et ON et.EventTypeId = pe.EventTypeId
            LEFT JOIN Location l ON l.LocationId = pe.LocationId
            WHERE pe.Slug = @Slug AND pe.IsDeleted = 0";

        var evt = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (evt is null)
            return NotFoundResult($"Event with slug '{slug}' not found.");

        return OkResult(evt);
    }

    // ──────────────────────────────────────────────
    //  Locations
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/locations — paginated location listing.
    /// </summary>
    [HttpGet("locations")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetLocations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Location WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                l.LocationId,
                l.Slug,
                l.Name,
                l.Description,
                lt.Name AS LocationTypeName,
                pl.Name AS ParentLocationName,
                pl.Slug AS ParentLocationSlug,
                c.Name AS CountryName
            FROM Location l
            LEFT JOIN LocationType lt ON lt.LocationTypeId = l.LocationTypeId
            LEFT JOIN Location pl ON pl.LocationId = l.ParentLocationId
            LEFT JOIN Country c ON c.CountryId = l.CountryId
            WHERE l.IsDeleted = 0
            ORDER BY l.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/locations/{slug} — location detail.
    /// </summary>
    [HttpGet("locations/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetLocationBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                l.LocationId,
                l.EntityId,
                l.Slug,
                l.Name,
                l.Description,
                lt.Name AS LocationTypeName,
                pl.Name AS ParentLocationName,
                pl.Slug AS ParentLocationSlug,
                c.Name AS CountryName
            FROM Location l
            LEFT JOIN LocationType lt ON lt.LocationTypeId = l.LocationTypeId
            LEFT JOIN Location pl ON pl.LocationId = l.ParentLocationId
            LEFT JOIN Country c ON c.CountryId = l.CountryId
            WHERE l.Slug = @Slug AND l.IsDeleted = 0";

        var location = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (location is null)
            return NotFoundResult($"Location with slug '{slug}' not found.");

        return OkResult(location);
    }

    // ──────────────────────────────────────────────
    //  Awards
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/awards — paginated award listing.
    /// </summary>
    [HttpGet("awards")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetAwards(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Award WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                a.AwardId,
                a.Slug,
                a.Name,
                a.Description,
                a.Organization AS OrganizationName,
                c.Name AS CountryName
            FROM Award a
            LEFT JOIN Country c ON c.CountryId = a.CountryId
            WHERE a.IsDeleted = 0
            ORDER BY a.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/awards/{slug} — award detail.
    /// </summary>
    [HttpGet("awards/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetAwardBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                a.AwardId,
                a.EntityId,
                a.Slug,
                a.Name,
                a.Description,
                a.Organization AS OrganizationName,
                c.Name AS CountryName
            FROM Award a
            LEFT JOIN Country c ON c.CountryId = a.CountryId
            WHERE a.Slug = @Slug AND a.IsDeleted = 0";

        var award = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (award is null)
            return NotFoundResult($"Award with slug '{slug}' not found.");

        return OkResult(award);
    }

    // ──────────────────────────────────────────────
    //  Certifications
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/certifications — paginated certification listing.
    /// </summary>
    [HttpGet("certifications")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetCertifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Certification WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                c.CertificationId,
                c.Slug,
                c.Name,
                c.Description,
                c.Organization AS OrganizationName,
                co.Name AS CountryName
            FROM Certification c
            LEFT JOIN Country co ON co.CountryId = c.CountryId
            WHERE c.IsDeleted = 0
            ORDER BY c.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    // ──────────────────────────────────────────────
    //  Charts
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/charts — paginated chart listing.
    /// </summary>
    [HttpGet("charts")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetCharts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Chart WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                c.ChartId,
                c.Slug,
                c.Name,
                c.Description,
                c.Publisher AS PublisherName,
                c.Frequency AS FrequencyName,
                co.Name AS CountryName
            FROM Chart c
            LEFT JOIN Country co ON co.CountryId = c.CountryId
            WHERE c.IsDeleted = 0
            ORDER BY c.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/charts/{slug} — chart detail.
    /// </summary>
    [HttpGet("charts/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetChartBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                c.ChartId,
                c.EntityId,
                c.Slug,
                c.Name,
                c.Description,
                c.Publisher AS PublisherName,
                c.Frequency AS FrequencyName,
                co.Name AS CountryName
            FROM Chart c
            LEFT JOIN Country co ON co.CountryId = c.CountryId
            WHERE c.Slug = @Slug AND c.IsDeleted = 0";

        var chart = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (chart is null)
            return NotFoundResult($"Chart with slug '{slug}' not found.");

        return OkResult(chart);
    }

    /// <summary>
    /// GET /api/v1/charts/{slug}/entries — chart entries for a specific chart.
    /// </summary>
    [HttpGet("charts/{slug}/entries")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["slug", "page", "pageSize"])]
    public async Task<IActionResult> GetChartEntries(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var chart = await connection.QueryFirstOrDefaultAsync(
            "SELECT ChartId FROM Chart WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (chart is null)
            return NotFoundResult($"Chart with slug '{slug}' not found.");

        var countSql = "SELECT COUNT(*) FROM ChartEntry WHERE ChartId = @ChartId";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { ChartId = chart.ChartId });

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                ce.ChartEntryId,
                ce.Date,
                ce.Position,
                ce.PreviousPosition,
                ce.WeeksOnChart,
                et.Code AS EntityTypeCode,
                ce.EntityId
            FROM ChartEntry ce
            INNER JOIN EntityType et ON et.EntityTypeId = ce.EntityTypeId
            WHERE ce.ChartId = @ChartId
            ORDER BY ce.Date DESC, ce.Position
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { ChartId = chart.ChartId, Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }
}
