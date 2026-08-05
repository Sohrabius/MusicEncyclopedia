using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin chart management controller.
/// Route: /admin/charts
/// Requires the CanManageCharts permission policy.
/// </summary>
[Route("/admin/charts")]
[Authorize(Policy = PermissionConstants.CanManageCharts)]
public sealed class ChartsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<ChartsController> _logger;

    public ChartsController(AppDbContext db, ILogger<ChartsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var query = _db.Charts
            .Include(c => c.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Slug.ToLower().Contains(term) ||
                (c.Publisher != null && c.Publisher.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var charts = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = charts.Select(c => new ChartListItemDto
        {
            ChartId = c.ChartId,
            Name = c.Name,
            Slug = c.Slug,
            Publisher = c.Publisher,
            CountryName = c.Country?.Name,
            Frequency = c.Frequency
        }).ToList();

        var viewModel = new ChartListViewModel
        {
            Items = PagedResult<ChartListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Charts";
        ViewData["ActiveMenu"] = "Charts";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new ChartEditViewModel
        {
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Chart";
        ViewData["ActiveMenu"] = "Charts";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ChartEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Chart";
            return View(viewModel);
        }

        var slugExists = await _db.Charts.AnyAsync(c => c.Slug == viewModel.Slug, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A chart with this slug already exists.");
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Chart";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 19, // Chart entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var chart = new Chart
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            Publisher = viewModel.Publisher,
            CountryId = viewModel.CountryId,
            Frequency = viewModel.Frequency,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Charts.Add(chart);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chart created: ChartId={Id}, Name={Name}, Slug={Slug}",
            chart.ChartId, chart.Name, chart.Slug);

        await InvalidateEntityCacheAsync("Chart", chart.ChartId, "Created");

        SetSuccessMessage($"Chart \"{chart.Name}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = chart.ChartId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var chart = await _db.Charts
            .FirstOrDefaultAsync(c => c.ChartId == id, cancellationToken);

        if (chart is null)
        {
            SetErrorMessage("Chart not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ChartEditViewModel
        {
            ChartId = chart.ChartId,
            Name = chart.Name,
            Publisher = chart.Publisher,
            CountryId = chart.CountryId,
            Frequency = chart.Frequency,
            Slug = chart.Slug,
            RowVersion = chart.RowVersion,
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {chart.Name}";
        ViewData["ActiveMenu"] = "Charts";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        ChartEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.ChartId)
        {
            SetErrorMessage("Chart ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var chart = await _db.Charts
            .FirstOrDefaultAsync(c => c.ChartId == id, cancellationToken);

        if (chart is null)
        {
            SetErrorMessage("Chart not found.");
            return RedirectToAction(nameof(Index));
        }

        var slugExists = await _db.Charts.AnyAsync(c => c.Slug == viewModel.Slug && c.ChartId != id, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A chart with this slug already exists.");
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        if (viewModel.RowVersion is not null)
        {
            _db.Entry(chart).Property(nameof(Chart.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        chart.Name = viewModel.Name;
        chart.Publisher = viewModel.Publisher;
        chart.CountryId = viewModel.CountryId;
        chart.Frequency = viewModel.Frequency;
        chart.Slug = viewModel.Slug;
        chart.ModifiedBy = User.Identity?.Name ?? "system";
        chart.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == chart.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Chart updated: ChartId={Id}, Name={Name}", chart.ChartId, chart.Name);

            await InvalidateEntityCacheAsync("Chart", chart.ChartId, "Updated");

            SetSuccessMessage($"Chart \"{chart.Name}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating chart {Id}", id);
            SetErrorMessage("This chart was modified by another user. Please reload and try again.");
            viewModel.RowVersion = chart.RowVersion;
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var chart = await _db.Charts
            .FirstOrDefaultAsync(c => c.ChartId == id, cancellationToken);

        if (chart is null)
        {
            SetErrorMessage("Chart not found.");
            return RedirectToAction(nameof(Index));
        }

        chart.IsDeleted = true;
        chart.ModifiedBy = User.Identity?.Name ?? "system";
        chart.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == chart.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Chart soft-deleted: ChartId={Id}, Name={Name}", id, chart.Name);

        await InvalidateEntityCacheAsync("Chart", chart.ChartId, "Deleted");

        SetSuccessMessage($"Chart \"{chart.Name}\" has been deleted.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var chart = await _db.Charts
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.ChartId == id, cancellationToken);

        if (chart is null)
        {
            SetErrorMessage("Chart not found.");
            return RedirectToAction(nameof(Index));
        }

        chart.IsDeleted = false;
        chart.ModifiedBy = User.Identity?.Name ?? "system";
        chart.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == chart.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Chart", chart.ChartId, "Restored");

        SetSuccessMessage($"Chart \"{chart.Name}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
