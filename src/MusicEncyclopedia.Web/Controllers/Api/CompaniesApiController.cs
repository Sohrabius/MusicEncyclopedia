using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for company resources.
/// Spec 11.1 — Companies endpoints.
/// </summary>
public sealed class CompaniesApiController : BaseApiController
{
    private readonly ICompanyQueryService _companyQueryService;
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CompaniesApiController> _logger;

    public CompaniesApiController(
        ICompanyQueryService companyQueryService,
        ICacheService cache,
        IConfiguration configuration,
        ILogger<CompaniesApiController> logger)
    {
        _companyQueryService = companyQueryService;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/companies — paginated company listing.
    /// Supports ?page, ?pageSize, ?q.
    /// </summary>
    [HttpGet("companies")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize", "q"])]
    public async Task<IActionResult> GetCompanies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        var result = await _companyQueryService.GetCompaniesAsync(
            culture: "en",
            page: page,
            pageSize: pageSize,
            q: q,
            cancellationToken: cancellationToken);

        return OkListResult(result);
    }

    /// <summary>
    /// GET /api/v1/companies/{slug} — company detail.
    /// </summary>
    [HttpGet("companies/{slug}")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetCompanyBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var company = await _companyQueryService.GetCompanyBySlugAsync(slug, "en", cancellationToken);
        if (company is null)
            return NotFoundResult($"Company with slug '{slug}' not found.");

        return OkResult(company);
    }

    /// <summary>
    /// GET /api/v1/companies/{slug}/albums — albums associated with a company.
    /// </summary>
    [HttpGet("companies/{slug}/albums")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetCompanyAlbums(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var company = await connection.QueryFirstOrDefaultAsync(
            "SELECT CompanyId, EntityId FROM Company WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (company is null)
            return NotFoundResult($"Company with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                a.AlbumId,
                a.Slug,
                a.Title,
                a.OriginalTitle,
                a.EnglishTitle,
                a.CategoryName,
                a.ReleaseDate,
                a.DurationSeconds,
                a.CoverUrl,
                ac.CompanyRole,
                ac.CatalogNumber,
                ac.Barcode
            FROM AlbumCompany ac
            INNER JOIN Album a ON a.AlbumId = ac.AlbumId
            WHERE ac.CompanyId = @CompanyId AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC";

        var albums = (await connection.QueryAsync(sql, new { CompanyId = company.CompanyId })).AsList();
        return OkListResult(albums);
    }

    /// <summary>
    /// GET /api/v1/companies/{slug}/credits — credits for a company.
    /// </summary>
    [HttpGet("companies/{slug}/credits")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetCompanyCredits(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var company = await connection.QueryFirstOrDefaultAsync(
            "SELECT CompanyId, EntityId FROM Company WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (company is null)
            return NotFoundResult($"Company with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                c.CreditId,
                c.EntityTypeCode,
                c.EntityId,
                cr.Name AS RoleName,
                cr.Code AS RoleCode,
                c.DisplayOrder,
                c.IsPrimary,
                c.Notes
            FROM Credit c
            LEFT JOIN CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
            WHERE c.CompanyId = @CompanyId
            ORDER BY c.DisplayOrder";

        var credits = (await connection.QueryAsync(sql, new { CompanyId = company.CompanyId })).AsList();
        return OkListResult(credits);
    }
}
