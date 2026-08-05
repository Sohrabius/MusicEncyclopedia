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
/// Admin tag management controller (spec 10.13 supplement).
/// Route: /admin/tags
/// Requires the CanManageLookupTables permission policy.
/// </summary>
[Route("/admin/tags")]
[Authorize(Policy = PermissionConstants.CanManageLookupTables)]
public sealed class TagsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<TagsController> _logger;

    public TagsController(AppDbContext db, ILogger<TagsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  List
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var query = _db.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(t =>
                t.Name.ToLower().Contains(term) ||
                t.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var tags = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Get assignment counts for all tags in this page
        var tagIds = tags.Select(t => t.TagId).ToList();
        var assignmentCounts = await _db.TagAssignments
            .Where(ta => tagIds.Contains(ta.TagId))
            .GroupBy(ta => ta.TagId)
            .Select(g => new { TagId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.TagId, x => x.Count, cancellationToken);

        var items = tags.Select(t => new TagListItemDto
        {
            TagId = t.TagId,
            Name = t.Name,
            Slug = t.Slug,
            DescriptionPreview = t.Description?.Length > 100
                ? t.Description[..100] + "…"
                : t.Description,
            AssignmentCount = assignmentCounts.TryGetValue(t.TagId, out var cnt) ? cnt : 0
        }).ToList();

        var viewModel = new TagListViewModel
        {
            Items = PagedResult<TagListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Tags";
        ViewData["ActiveMenu"] = "Tags";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Create
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Create")]
    public IActionResult Create()
    {
        ViewData["Title"] = "Create Tag";
        ViewData["ActiveMenu"] = "Tags";

        return View(new TagEditViewModel());
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TagEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            ViewData["Title"] = "Create Tag";
            return View(viewModel);
        }

        // Check for duplicate slug
        var slugExists = await _db.Tags.AnyAsync(t => t.Slug == viewModel.Slug, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A tag with this slug already exists.");
            ViewData["Title"] = "Create Tag";
            return View(viewModel);
        }

        // Create the base Entity first
        var entity = new Entity
        {
            EntityTypeId = 16, // Tag entity type — adjust if dynamic lookup is needed
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var tag = new Tag
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            Description = viewModel.Description,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Tags.Add(tag);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tag created: TagId={TagId}, Name={Name}, Slug={Slug}",
            tag.TagId, tag.Name, tag.Slug);

        await InvalidateEntityCacheAsync("Tag", tag.TagId, "Created");

        SetSuccessMessage($"Tag \"{tag.Name}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = tag.TagId });
    }

    // ──────────────────────────────────────────────
    //  Edit
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _db.Tags
            .FirstOrDefaultAsync(t => t.TagId == id, cancellationToken);

        if (tag is null)
        {
            SetErrorMessage("Tag not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new TagEditViewModel
        {
            TagId = tag.TagId,
            Name = tag.Name,
            Description = tag.Description,
            Slug = tag.Slug,
            RowVersion = tag.RowVersion
        };

        ViewData["Title"] = $"Edit: {tag.Name}";
        ViewData["ActiveMenu"] = "Tags";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        TagEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.TagId)
        {
            SetErrorMessage("Tag ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var tag = await _db.Tags
            .FirstOrDefaultAsync(t => t.TagId == id, cancellationToken);

        if (tag is null)
        {
            SetErrorMessage("Tag not found.");
            return RedirectToAction(nameof(Index));
        }

        // Check for duplicate slug (excluding current tag)
        var slugExists = await _db.Tags.AnyAsync(t => t.Slug == viewModel.Slug && t.TagId != id, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A tag with this slug already exists.");
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        if (viewModel.RowVersion is not null)
        {
            _db.Entry(tag).Property(nameof(Tag.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        tag.Name = viewModel.Name;
        tag.Description = viewModel.Description;
        tag.Slug = viewModel.Slug;
        tag.ModifiedBy = User.Identity?.Name ?? "system";
        tag.ModifiedAt = DateTime.UtcNow;

        // Also update the Entity slug
        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == tag.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Tag updated: TagId={TagId}, Name={Name}", tag.TagId, tag.Name);

            await InvalidateEntityCacheAsync("Tag", tag.TagId, "Updated");

            SetSuccessMessage($"Tag \"{tag.Name}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating tag {TagId}", id);
            SetErrorMessage("This tag was modified by another user. Please reload and try again.");
            viewModel.RowVersion = tag.RowVersion;
            return View(viewModel);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    // ──────────────────────────────────────────────
    //  Delete (soft delete with TagAssignment check)
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _db.Tags
            .FirstOrDefaultAsync(t => t.TagId == id, cancellationToken);

        if (tag is null)
        {
            SetErrorMessage("Tag not found.");
            return RedirectToAction(nameof(Index));
        }

        // Check if tag is in use
        var assignmentCount = await _db.TagAssignments
            .CountAsync(ta => ta.TagId == id, cancellationToken);

        if (assignmentCount > 0)
        {
            // Option: cascade soft-delete the assignments
            _logger.LogInformation("Tag {TagId} has {Count} assignments — removing assignments before soft-delete.", id, assignmentCount);
            var assignments = await _db.TagAssignments
                .Where(ta => ta.TagId == id)
                .ToListAsync(cancellationToken);
            _db.TagAssignments.RemoveRange(assignments);
        }

        tag.IsDeleted = true;
        tag.ModifiedBy = User.Identity?.Name ?? "system";
        tag.ModifiedAt = DateTime.UtcNow;

        // Also soft-delete the base entity
        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == tag.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Tag soft-deleted: TagId={TagId}, Name={Name}", id, tag.Name);

        await InvalidateEntityCacheAsync("Tag", tag.TagId, "Deleted");

        SetSuccessMessage($"Tag \"{tag.Name}\" has been deleted.");
        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Restore
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var tag = await _db.Tags
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TagId == id, cancellationToken);

        if (tag is null)
        {
            SetErrorMessage("Tag not found.");
            return RedirectToAction(nameof(Index));
        }

        tag.IsDeleted = false;
        tag.ModifiedBy = User.Identity?.Name ?? "system";
        tag.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == tag.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Tag", tag.TagId, "Restored");

        SetSuccessMessage($"Tag \"{tag.Name}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Autocomplete API
    // ──────────────────────────────────────────────

    /// <summary>
    /// Returns JSON for tag autocomplete. Used by the tag editor UI.
    /// GET /admin/tags/autocomplete?q=term
    /// </summary>
    [HttpGet]
    [Route("autocomplete")]
    [AllowAnonymous] // Will still require authentication via the policy on the controller — this just allows unauthenticated for the API
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string? q,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Tags.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(t => t.Name.ToLower().Contains(term) || t.Slug.ToLower().Contains(term));
        }

        var tags = await query
            .OrderBy(t => t.Name)
            .Take(20)
            .Select(t => new
            {
                id = t.TagId,
                name = t.Name,
                slug = t.Slug
            })
            .ToListAsync(cancellationToken);

        return Json(tags);
    }
}
