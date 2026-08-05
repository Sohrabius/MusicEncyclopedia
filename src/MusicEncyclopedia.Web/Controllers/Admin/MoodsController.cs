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
/// Admin mood management controller.
/// Route: /admin/moods
/// Requires the CanManageMoods permission policy.
/// </summary>
[Route("/admin/moods")]
[Authorize(Policy = PermissionConstants.CanManageMoods)]
public sealed class MoodsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<MoodsController> _logger;

    public MoodsController(AppDbContext db, ILogger<MoodsController> logger)
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

        var query = _db.Moods.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(term) ||
                m.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var moods = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = moods.Select(m => new MoodListItemDto
        {
            MoodId = m.MoodId,
            Slug = m.Slug,
            Name = m.Name
        }).ToList();

        var viewModel = new MoodListViewModel
        {
            Items = PagedResult<MoodListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Moods";
        ViewData["ActiveMenu"] = "Moods";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public IActionResult Create()
    {
        var viewModel = new MoodEditViewModel();

        ViewData["Title"] = "Create Mood";
        ViewData["ActiveMenu"] = "Moods";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        MoodEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Mood";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 6, // Mood entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var mood = new Mood
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            Description = viewModel.Description,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Moods.Add(mood);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Mood created: MoodId={MoodId}, Name={Name}", mood.MoodId, mood.Name);

        await InvalidateEntityCacheAsync("Mood", mood.MoodId, "Created");

        SetSuccessMessage($"Mood \"{mood.Name}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = mood.MoodId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var mood = await _db.Moods
            .FirstOrDefaultAsync(m => m.MoodId == id, cancellationToken);

        if (mood is null)
        {
            SetErrorMessage("Mood not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new MoodEditViewModel
        {
            MoodId = mood.MoodId,
            Name = mood.Name,
            Description = mood.Description,
            Slug = mood.Slug,
            RowVersion = mood.RowVersion
        };

        ViewData["Title"] = $"Edit: {mood.Name}";
        ViewData["ActiveMenu"] = "Moods";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        MoodEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.MoodId)
        {
            SetErrorMessage("Mood ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var mood = await _db.Moods
            .FirstOrDefaultAsync(m => m.MoodId == id, cancellationToken);

        if (mood is null)
        {
            SetErrorMessage("Mood not found.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(mood).Property(nameof(Mood.RowVersion)).OriginalValue = viewModel.RowVersion;

        mood.Name = viewModel.Name;
        mood.Description = viewModel.Description;
        mood.Slug = viewModel.Slug;
        mood.ModifiedBy = User.Identity?.Name ?? "system";
        mood.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == mood.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Mood", mood.MoodId, "Updated");

            SetSuccessMessage($"Mood \"{mood.Name}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating mood {MoodId}", id);
            SetErrorMessage("This mood was modified by another user. Please reload and try again.");
            viewModel.RowVersion = mood.RowVersion;
            return View(viewModel);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var mood = await _db.Moods.FirstOrDefaultAsync(m => m.MoodId == id, cancellationToken);
        if (mood is null)
        {
            SetErrorMessage("Mood not found.");
            return RedirectToAction(nameof(Index));
        }

        mood.IsDeleted = true;
        mood.ModifiedBy = User.Identity?.Name ?? "system";
        mood.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == mood.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Mood", mood.MoodId, "Deleted");

        SetSuccessMessage($"Mood \"{mood.Name}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var mood = await _db.Moods.IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.MoodId == id, cancellationToken);
        if (mood is null)
        {
            SetErrorMessage("Mood not found.");
            return RedirectToAction(nameof(Index));
        }

        mood.IsDeleted = false;
        mood.ModifiedBy = User.Identity?.Name ?? "system";
        mood.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == mood.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Mood", mood.MoodId, "Restored");

        SetSuccessMessage($"Mood \"{mood.Name}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
