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
/// Admin sung version management controller (spec 10.10).
/// Route: /admin/sung-versions
/// Requires the CanManagePoems permission policy.
/// </summary>
[Route("/admin/sung-versions")]
[Authorize(Policy = PermissionConstants.CanManagePoems)]
public sealed class SungVersionsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<SungVersionsController> _logger;

    public SungVersionsController(AppDbContext db, ILogger<SungVersionsController> logger)
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

        var query = _db.SungVersions
            .Include(sv => sv.Poem)
            .Include(sv => sv.VocalStyle)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(sv =>
                sv.Title.ToLower().Contains(term) ||
                sv.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var sungVersions = await query
            .OrderByDescending(sv => sv.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = sungVersions.Select(sv => new SungVersionListItemDto
        {
            SungVersionId = sv.SungVersionId,
            Slug = sv.Slug,
            Title = sv.Title,
            PoemTitle = sv.Poem?.Title,
            VocalStyleName = sv.VocalStyle?.Name,
            IsCanonical = sv.IsCanonical
        }).ToList();

        var viewModel = new SungVersionListViewModel
        {
            Items = PagedResult<SungVersionListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Sung Versions";
        ViewData["ActiveMenu"] = "Sung Versions";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new SungVersionEditViewModel
        {
            Poems = await _db.Poems.OrderBy(p => p.Title).ToListAsync(cancellationToken),
            VocalStyles = await _db.VocalStyles.OrderBy(v => v.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Sung Version";
        ViewData["ActiveMenu"] = "Sung Versions";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SungVersionEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Poems = await _db.Poems.OrderBy(p => p.Title).ToListAsync(cancellationToken);
            viewModel.VocalStyles = await _db.VocalStyles.OrderBy(v => v.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Sung Version";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 9, // SungVersion entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var sungVersion = new SungVersion
        {
            EntityId = entity.EntityId,
            PoemId = viewModel.PoemId,
            Title = viewModel.Title,
            VocalStyleId = viewModel.VocalStyleId,
            Text = viewModel.Text,
            Notes = viewModel.Notes,
            IsCanonical = viewModel.IsCanonical,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.SungVersions.Add(sungVersion);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("SungVersion created: SungVersionId={SungVersionId}, Title={Title}",
            sungVersion.SungVersionId, sungVersion.Title);

        await InvalidateEntityCacheAsync("SungVersion", sungVersion.SungVersionId, "Created");

        SetSuccessMessage($"Sung version \"{sungVersion.Title}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = sungVersion.SungVersionId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var sungVersion = await _db.SungVersions
            .FirstOrDefaultAsync(sv => sv.SungVersionId == id, cancellationToken);

        if (sungVersion is null)
        {
            SetErrorMessage("Sung version not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new SungVersionEditViewModel
        {
            SungVersionId = sungVersion.SungVersionId,
            Title = sungVersion.Title,
            PoemId = sungVersion.PoemId,
            VocalStyleId = sungVersion.VocalStyleId,
            Text = sungVersion.Text,
            Notes = sungVersion.Notes,
            IsCanonical = sungVersion.IsCanonical,
            Slug = sungVersion.Slug,
            RowVersion = sungVersion.RowVersion,
            Poems = await _db.Poems.OrderBy(p => p.Title).ToListAsync(cancellationToken),
            VocalStyles = await _db.VocalStyles.OrderBy(v => v.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {sungVersion.Title}";
        ViewData["ActiveMenu"] = "Sung Versions";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        SungVersionEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.SungVersionId)
        {
            SetErrorMessage("Sung version ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.Poems = await _db.Poems.OrderBy(p => p.Title).ToListAsync(cancellationToken);
            viewModel.VocalStyles = await _db.VocalStyles.OrderBy(v => v.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Title}";
            return View(viewModel);
        }

        var sungVersion = await _db.SungVersions
            .FirstOrDefaultAsync(sv => sv.SungVersionId == id, cancellationToken);

        if (sungVersion is null)
        {
            SetErrorMessage("Sung version not found.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(sungVersion).Property(nameof(SungVersion.RowVersion)).OriginalValue = viewModel.RowVersion;

        sungVersion.Title = viewModel.Title;
        sungVersion.PoemId = viewModel.PoemId;
        sungVersion.VocalStyleId = viewModel.VocalStyleId;
        sungVersion.Text = viewModel.Text;
        sungVersion.Notes = viewModel.Notes;
        sungVersion.IsCanonical = viewModel.IsCanonical;
        sungVersion.Slug = viewModel.Slug;
        sungVersion.ModifiedBy = User.Identity?.Name ?? "system";
        sungVersion.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == sungVersion.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("SungVersion", sungVersion.SungVersionId, "Updated");

            SetSuccessMessage($"Sung version \"{sungVersion.Title}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating sung version {SungVersionId}", id);
            SetErrorMessage("This sung version was modified by another user. Please reload and try again.");
            viewModel.Poems = await _db.Poems.OrderBy(p => p.Title).ToListAsync(cancellationToken);
            viewModel.VocalStyles = await _db.VocalStyles.OrderBy(v => v.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = sungVersion.RowVersion;
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
        var sungVersion = await _db.SungVersions.FirstOrDefaultAsync(sv => sv.SungVersionId == id, cancellationToken);
        if (sungVersion is null)
        {
            SetErrorMessage("Sung version not found.");
            return RedirectToAction(nameof(Index));
        }

        sungVersion.IsDeleted = true;
        sungVersion.ModifiedBy = User.Identity?.Name ?? "system";
        sungVersion.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == sungVersion.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("SungVersion", sungVersion.SungVersionId, "Deleted");

        SetSuccessMessage($"Sung version \"{sungVersion.Title}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var sungVersion = await _db.SungVersions.IgnoreQueryFilters()
            .FirstOrDefaultAsync(sv => sv.SungVersionId == id, cancellationToken);
        if (sungVersion is null)
        {
            SetErrorMessage("Sung version not found.");
            return RedirectToAction(nameof(Index));
        }

        sungVersion.IsDeleted = false;
        sungVersion.ModifiedBy = User.Identity?.Name ?? "system";
        sungVersion.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == sungVersion.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("SungVersion", sungVersion.SungVersionId, "Restored");

        SetSuccessMessage($"Sung version \"{sungVersion.Title}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
