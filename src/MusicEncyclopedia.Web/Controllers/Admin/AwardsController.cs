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
/// Admin award management controller.
/// Route: /admin/awards
/// Requires the CanManageAwards permission policy.
/// </summary>
[Route("/admin/awards")]
[Authorize(Policy = PermissionConstants.CanManageAwards)]
public sealed class AwardsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<AwardsController> _logger;

    public AwardsController(AppDbContext db, ILogger<AwardsController> logger)
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

        var query = _db.Awards
            .Include(a => a.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(a =>
                a.Name.ToLower().Contains(term) ||
                a.Slug.ToLower().Contains(term) ||
                (a.Organization != null && a.Organization.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var awards = await query
            .OrderBy(a => a.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = awards.Select(a => new AwardListItemDto
        {
            AwardId = a.AwardId,
            Name = a.Name,
            Slug = a.Slug,
            Organization = a.Organization,
            CountryName = a.Country?.Name
        }).ToList();

        var viewModel = new AwardListViewModel
        {
            Items = PagedResult<AwardListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Awards";
        ViewData["ActiveMenu"] = "Awards";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new AwardEditViewModel
        {
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Award";
        ViewData["ActiveMenu"] = "Awards";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AwardEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Award";
            return View(viewModel);
        }

        var slugExists = await _db.Awards.AnyAsync(a => a.Slug == viewModel.Slug, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "An award with this slug already exists.");
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Award";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 17, // Award entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var award = new Award
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            Organization = viewModel.Organization,
            CountryId = viewModel.CountryId,
            Description = viewModel.Description,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Awards.Add(award);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Award created: AwardId={Id}, Name={Name}, Slug={Slug}",
            award.AwardId, award.Name, award.Slug);

        await InvalidateEntityCacheAsync("Award", award.AwardId, "Created");

        SetSuccessMessage($"جایزه «{award.Name}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = award.AwardId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var award = await _db.Awards
            .FirstOrDefaultAsync(a => a.AwardId == id, cancellationToken);

        if (award is null)
        {
            SetErrorMessage("جایزه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new AwardEditViewModel
        {
            AwardId = award.AwardId,
            Name = award.Name,
            Organization = award.Organization,
            CountryId = award.CountryId,
            Description = award.Description,
            Slug = award.Slug,
            RowVersion = award.RowVersion,
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {award.Name}";
        ViewData["ActiveMenu"] = "Awards";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AwardEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.AwardId)
        {
            SetErrorMessage("شناسه جایزه ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var award = await _db.Awards
            .FirstOrDefaultAsync(a => a.AwardId == id, cancellationToken);

        if (award is null)
        {
            SetErrorMessage("جایزه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var slugExists = await _db.Awards.AnyAsync(a => a.Slug == viewModel.Slug && a.AwardId != id, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "An award with this slug already exists.");
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        if (viewModel.RowVersion is not null)
        {
            _db.Entry(award).Property(nameof(Award.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        award.Name = viewModel.Name;
        award.Organization = viewModel.Organization;
        award.CountryId = viewModel.CountryId;
        award.Description = viewModel.Description;
        award.Slug = viewModel.Slug;
        award.ModifiedBy = User.Identity?.Name ?? "system";
        award.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == award.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Award updated: AwardId={Id}, Name={Name}", award.AwardId, award.Name);

            await InvalidateEntityCacheAsync("Award", award.AwardId, "Updated");

            SetSuccessMessage($"جایزه «{award.Name}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating award {Id}", id);
            SetErrorMessage("این جایزه توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.RowVersion = award.RowVersion;
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
        var award = await _db.Awards
            .FirstOrDefaultAsync(a => a.AwardId == id, cancellationToken);

        if (award is null)
        {
            SetErrorMessage("جایزه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        award.IsDeleted = true;
        award.ModifiedBy = User.Identity?.Name ?? "system";
        award.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == award.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Award soft-deleted: AwardId={Id}, Name={Name}", id, award.Name);

        await InvalidateEntityCacheAsync("Award", award.AwardId, "Deleted");

        SetSuccessMessage($"جایزه «{award.Name}» حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var award = await _db.Awards
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.AwardId == id, cancellationToken);

        if (award is null)
        {
            SetErrorMessage("جایزه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        award.IsDeleted = false;
        award.ModifiedBy = User.Identity?.Name ?? "system";
        award.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == award.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Award", award.AwardId, "Restored");

        SetSuccessMessage($"جایزه «{award.Name}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
