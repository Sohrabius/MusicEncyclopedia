using System.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for award listing and detail pages.
/// Routes: /{culture}/awards
/// Spec reference: 9.17
/// </summary>
[Route("{culture:regex(^(fa)$)}/awards")]
public sealed class AwardsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<AwardsController> _logger;

    public AwardsController(IDbConnection db, ILogger<AwardsController> logger)
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

        var countSql = "SELECT COUNT(*) FROM Award a WHERE a.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                a.AwardId,
                a.Name,
                a.Slug,
                a.Organization,
                c.Name AS CountryName,
                a.Description AS DescriptionPreview
            FROM Award a
            LEFT JOIN Country c ON a.CountryId = c.CountryId
            WHERE a.IsDeleted = 0
            ORDER BY a.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await _db.QueryAsync<AwardListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();

        // Truncate DescriptionPreview to 200 chars (LEFT() is SQL Server-specific; SQLite uses SUBSTR)
        foreach (var item in items)
        {
            if (item.DescriptionPreview?.Length > 200)
            {
                item.DescriptionPreview = item.DescriptionPreview[..200] + "...";
            }
        }

        var result = MusicEncyclopedia.Core.DTOs.PagedResult<AwardListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new AwardListViewModel { Items = result, Culture = culture };

        ViewData["Title"] = "Awards";
        ViewData["MetaDescription"] = "Browse awards and honors — organizations, winners, nominees, and more.";

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
        var awardSql = @"
            SELECT 
                a.AwardId,
                a.Name,
                a.Slug,
                a.Organization,
                c.Name AS CountryName,
                a.Description
            FROM Award a
            LEFT JOIN Country c ON a.CountryId = c.CountryId
            WHERE a.Slug = @Slug AND a.IsDeleted = 0";

        var award = await _db.QuerySingleOrDefaultAsync<AwardDetailDto>(awardSql, new { Slug = slug });

        if (award is null)
        {
            _logger.LogWarning("Award not found: slug={Slug}", slug);
            return NotFound();
        }

        // Award assignments (winners/nominees)
        var assignmentsSql = @"
            SELECT 
                aa.AwardAssignmentId,
                aa.EntityTypeId,
                aa.EntityId,
                aa.Year,
                aa.Category,
                aa.Notes,
                art.Name AS ResultTypeName,
                art.Code AS ResultTypeCode
            FROM AwardAssignment aa
            LEFT JOIN AwardResultType art ON aa.AwardResultTypeId = art.AwardResultTypeId
            WHERE aa.AwardId = @AwardId
            ORDER BY aa.Year DESC, aa.Category";
        var assignments = (await _db.QueryAsync<AwardAssignmentDto>(assignmentsSql, new { AwardId = award.AwardId })).ToList();

        // Resolve entity titles for each assignment
        for (int i = 0; i < assignments.Count; i++)
        {
            var assignment = assignments[i];
            string? titleSql = assignment.EntityTypeId switch
            {
                1 => "SELECT Title AS Name FROM Album WHERE EntityId = @Eid",
                2 => "SELECT Title AS Name FROM Track WHERE EntityId = @Eid",
                3 => "SELECT FullName AS Name FROM Person WHERE EntityId = @Eid",
                4 => "SELECT Name AS Name FROM Company WHERE EntityId = @Eid",
                _ => null
            };

            if (titleSql is not null)
            {
                var entityName = await _db.QuerySingleOrDefaultAsync<string>(titleSql, new { Eid = assignment.EntityId });
                assignments[i] = assignment with { EntityName = entityName ?? "Unknown" };
            }
        }

        award = award with { Assignments = assignments };

        var viewModel = new AwardDetailViewModel { Award = award, Culture = culture };

        ViewData["Title"] = award.Name;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(award.Description)
            ? (award.Description.Length > 200 ? award.Description[..200] + "..." : award.Description)
            : $"Award: {award.Name}";
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/awards/{slug}";

        return View(viewModel);
    }
}
