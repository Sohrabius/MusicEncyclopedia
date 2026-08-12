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
/// Admin entity link management controller (spec 10.14 supplement).
/// Route: /admin/links
/// Requires the CanManageAlbums permission policy (links are entity-agnostic).
/// </summary>
[Route("/admin/links")]
[Authorize(Policy = PermissionConstants.CanManageAlbums)]
public sealed class LinksController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<LinksController> _logger;

    public LinksController(AppDbContext db, ILogger<LinksController> logger)
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
        int? entityTypeId = null,
        int? entityId = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var query = _db.EntityLinks
            .Include(el => el.LinkType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(el =>
                el.Url.ToLower().Contains(term) ||
                (el.Title != null && el.Title.ToLower().Contains(term)));
        }

        if (entityTypeId.HasValue)
        {
            query = query.Where(el => el.EntityTypeId == entityTypeId.Value);
        }

        if (entityId.HasValue)
        {
            query = query.Where(el => el.EntityId == entityId.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var links = await query
            .OrderByDescending(el => el.EntityLinkId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = links.Select(el => new LinkListItemDto
        {
            EntityLinkId = el.EntityLinkId,
            EntityTypeName = null, // Could join EntityType if needed
            EntityId = el.EntityId,
            LinkTypeName = el.LinkType?.Name,
            Url = el.Url,
            Title = el.Title
        }).ToList();

        var viewModel = new LinkListViewModel
        {
            Items = PagedResult<LinkListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            EntityTypeIdFilter = entityTypeId,
            EntityIdFilter = entityId,
            Page = page,
            PageSize = pageSize,
            EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Entity Links";
        ViewData["ActiveMenu"] = "Links";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Create
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new LinkEditViewModel
        {
            EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken),
            LinkTypes = await _db.LinkTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Link";
        ViewData["ActiveMenu"] = "Links";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        LinkEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.LinkTypes = await _db.LinkTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Link";
            return View(viewModel);
        }

        var entityLink = new EntityLink
        {
            EntityTypeId = viewModel.EntityTypeId,
            EntityId = viewModel.EntityId,
            LinkTypeId = viewModel.LinkTypeId,
            Url = viewModel.Url,
            Title = viewModel.Title,
            IsDeleted = false
        };

        _db.EntityLinks.Add(entityLink);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("EntityLink created: EntityLinkId={Id}, EntityTypeId={EtId}, EntityId={EId}",
            entityLink.EntityLinkId, entityLink.EntityTypeId, entityLink.EntityId);

        await InvalidateEntityCacheAsync(entityLink.EntityTypeId, entityLink.EntityId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("پیوند با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = entityLink.EntityLinkId });
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
        var entityLink = await _db.EntityLinks
            .FirstOrDefaultAsync(el => el.EntityLinkId == id, cancellationToken);

        if (entityLink is null)
        {
            SetErrorMessage("پیوند یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new LinkEditViewModel
        {
            EntityLinkId = entityLink.EntityLinkId,
            EntityTypeId = entityLink.EntityTypeId,
            EntityId = entityLink.EntityId,
            LinkTypeId = entityLink.LinkTypeId,
            Url = entityLink.Url,
            Title = entityLink.Title,
            EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken),
            LinkTypes = await _db.LinkTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Edit Link";
        ViewData["ActiveMenu"] = "Links";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        LinkEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.EntityLinkId)
        {
            SetErrorMessage("شناسه پیوند ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.LinkTypes = await _db.LinkTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Edit Link";
            return View(viewModel);
        }

        var entityLink = await _db.EntityLinks
            .FirstOrDefaultAsync(el => el.EntityLinkId == id, cancellationToken);

        if (entityLink is null)
        {
            SetErrorMessage("پیوند یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        entityLink.EntityTypeId = viewModel.EntityTypeId;
        entityLink.EntityId = viewModel.EntityId;
        entityLink.LinkTypeId = viewModel.LinkTypeId;
        entityLink.Url = viewModel.Url;
        entityLink.Title = viewModel.Title;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("EntityLink updated: EntityLinkId={Id}", id);

        if (entityLink is not null)
        {
            await InvalidateEntityCacheAsync(entityLink.EntityTypeId, entityLink.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("پیوند با موفقیت به‌روزرسانی شد.");

        return RedirectToAction(nameof(Edit), new { id });
    }

    // ──────────────────────────────────────────────
    //  Delete (hard delete — EntityLink has IsDeleted but we set it)
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var entityLink = await _db.EntityLinks
            .FirstOrDefaultAsync(el => el.EntityLinkId == id, cancellationToken);

        if (entityLink is null)
        {
            SetErrorMessage("پیوند یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        entityLink.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("EntityLink soft-deleted: EntityLinkId={Id}", id);

        if (entityLink is not null)
        {
            await InvalidateEntityCacheAsync(entityLink.EntityTypeId, entityLink.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("پیوند با موفقیت حذف شد.");

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
        var entityLink = await _db.EntityLinks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(el => el.EntityLinkId == id, cancellationToken);

        if (entityLink is null)
        {
            SetErrorMessage("پیوند یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        entityLink.IsDeleted = false;
        await _db.SaveChangesAsync(cancellationToken);

        if (entityLink is not null)
        {
            await InvalidateEntityCacheAsync(entityLink.EntityTypeId, entityLink.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("پیوند با موفقیت بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
