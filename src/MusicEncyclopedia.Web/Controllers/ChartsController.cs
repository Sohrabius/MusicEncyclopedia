using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for chart listing and detail pages.
/// Routes: /{culture}/charts
/// Spec reference: 9.18
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/charts")]
public sealed class ChartsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<ChartsController> _logger;

    public ChartsController(IDbConnection db, ILogger<ChartsController> logger)
    {
        _db = db;
        _logger = logger;
    }

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

        var countSql = "SELECT COUNT(*) FROM Chart c WHERE c.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                c.ChartId,
                c.Name,
                c.Slug,
                c.Publisher,
                co.Name AS CountryName,
                c.Frequency
            FROM Chart c
            LEFT JOIN Country co ON c.CountryId = co.CountryId
            WHERE c.IsDeleted = 0
            ORDER BY c.Name
            " + SqlDialect.Pagination();

        var items = (await _db.QueryAsync<ChartListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();
        var result = MusicEncyclopedia.Core.DTOs.PagedResult<ChartListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new ChartListViewModel { Items = result, Culture = culture };

        ViewData["Title"] = "Charts";
        ViewData["MetaDescription"] = "Browse music charts — publishers, rankings, entries, and more.";

        return View(viewModel);
    }

    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        var chartSql = @"
            SELECT 
                c.ChartId,
                c.Name,
                c.Slug,
                c.Publisher,
                co.Name AS CountryName,
                c.Frequency
            FROM Chart c
            LEFT JOIN Country co ON c.CountryId = co.CountryId
            WHERE c.Slug = @Slug AND c.IsDeleted = 0";

        var chart = await _db.QuerySingleOrDefaultAsync<ChartDetailDto>(chartSql, new { Slug = slug });

        if (chart is null)
        {
            _logger.LogWarning("Chart not found: slug={Slug}", slug);
            return NotFound();
        }

        // Chart entries
        var entriesSql = @"
            SELECT 
                ce.ChartEntryId,
                ce.Date,
                ce.Position,
                ce.PreviousPosition,
                ce.WeeksOnChart,
                ce.EntityTypeId,
                ce.EntityId
            FROM ChartEntry ce
            WHERE ce.ChartId = @ChartId
            ORDER BY ce.Date DESC, ce.Position";

        var entries = (await _db.QueryAsync<ChartEntryDetailDto>(entriesSql, new { ChartId = chart.ChartId })).ToList();

        // Resolve entity titles for each entry
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            string? titleSql = entry.EntityTypeId switch
            {
                1 => "SELECT Title AS Name, Slug FROM Album WHERE EntityId = @Eid",
                2 => "SELECT Title AS Name, Slug FROM Track WHERE EntityId = @Eid",
                3 => "SELECT FullName AS Name, Slug FROM Person WHERE EntityId = @Eid",
                _ => null
            };

            if (titleSql is not null)
            {
                var entity = await _db.QuerySingleOrDefaultAsync(titleSql, new { Eid = entry.EntityId });
                if (entity is not null)
                {
                    var dict = (IDictionary<string, object>)entity;
                    entries[i] = entry with
                    {
                        EntityTitle = dict["Name"]?.ToString(),
                        EntitySlug = dict["Slug"]?.ToString()
                    };
                }
            }
        }

        chart = chart with { Entries = entries };

        var viewModel = new ChartDetailViewModel { Chart = chart, Culture = culture };

        ViewData["Title"] = chart.Name;
        ViewData["MetaDescription"] = $"Chart: {chart.Name}" + (!string.IsNullOrWhiteSpace(chart.Publisher) ? $" by {chart.Publisher}" : "");
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/charts/{slug}";

        return View(viewModel);
    }
}
