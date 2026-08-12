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
/// Admin localization management controller (spec 10.12).
/// Route: /admin/localizations
/// Requires the CanManageLocalizations permission policy.
/// </summary>
[Route("/admin/localizations")]
[Authorize(Policy = PermissionConstants.CanManageLocalizations)]
public sealed class LocalizationsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<LocalizationsController> _logger;

    public LocalizationsController(AppDbContext db, ILogger<LocalizationsController> logger)
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

        var query = _db.Localizations
            .Include(l => l.Language)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(l =>
                l.FieldName.ToLower().Contains(term) ||
                l.LocalizedText.ToLower().Contains(term) ||
                l.EntityId.ToString().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var localizations = await query
            .OrderByDescending(l => l.LocalizationId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = localizations.Select(l => new LocalizationListItemDto
        {
            LocalizationId = l.LocalizationId,
            EntityTypeName = null, // Could be looked up if needed
            EntityId = l.EntityId,
            LanguageName = l.Language?.Name,
            LanguageCode = l.Language?.Code,
            FieldName = l.FieldName,
            LocalizedTextPreview = l.LocalizedText.Length > 100
                ? l.LocalizedText[..100] + "…"
                : l.LocalizedText
        }).ToList();

        var viewModel = new LocalizationListViewModel
        {
            Items = PagedResult<LocalizationListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Localizations";
        ViewData["ActiveMenu"] = "Localizations";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Create
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new LocalizationEditViewModel
        {
            EntityTypes = await _db.EntityTypes.OrderBy(e => e.Name).ToListAsync(cancellationToken),
            Languages = await _db.Languages.OrderBy(l => l.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Localization";
        ViewData["ActiveMenu"] = "Localizations";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        LocalizationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.EntityTypes = await _db.EntityTypes.OrderBy(e => e.Name).ToListAsync(cancellationToken);
            viewModel.Languages = await _db.Languages.OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Localization";
            return View(viewModel);
        }

        var localization = new Localization
        {
            EntityTypeId = viewModel.EntityTypeId,
            EntityId = viewModel.EntityId,
            LanguageId = viewModel.LanguageId,
            FieldName = viewModel.FieldName,
            LocalizedText = viewModel.LocalizedText
        };

        _db.Localizations.Add(localization);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Localization created: LocalizationId={Id}, EntityTypeId={EtId}, EntityId={EId}, Field={Field}",
            localization.LocalizationId, localization.EntityTypeId, localization.EntityId, localization.FieldName);

        await InvalidateEntityCacheAsync(localization.EntityTypeId, localization.EntityId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("بومی‌سازی با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Index));
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
        var localization = await _db.Localizations
            .FirstOrDefaultAsync(l => l.LocalizationId == id, cancellationToken);

        if (localization is null)
        {
            SetErrorMessage("بومی‌سازی یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new LocalizationEditViewModel
        {
            LocalizationId = localization.LocalizationId,
            EntityTypeId = localization.EntityTypeId,
            EntityId = localization.EntityId,
            LanguageId = localization.LanguageId,
            FieldName = localization.FieldName,
            LocalizedText = localization.LocalizedText,
            EntityTypes = await _db.EntityTypes.OrderBy(e => e.Name).ToListAsync(cancellationToken),
            Languages = await _db.Languages.OrderBy(l => l.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Edit Localization";
        ViewData["ActiveMenu"] = "Localizations";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        LocalizationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.LocalizationId)
        {
            SetErrorMessage("شناسه بومی‌سازی ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.EntityTypes = await _db.EntityTypes.OrderBy(e => e.Name).ToListAsync(cancellationToken);
            viewModel.Languages = await _db.Languages.OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Edit Localization";
            return View(viewModel);
        }

        var localization = await _db.Localizations
            .FirstOrDefaultAsync(l => l.LocalizationId == id, cancellationToken);

        if (localization is null)
        {
            SetErrorMessage("بومی‌سازی یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        localization.EntityTypeId = viewModel.EntityTypeId;
        localization.EntityId = viewModel.EntityId;
        localization.LanguageId = viewModel.LanguageId;
        localization.FieldName = viewModel.FieldName;
        localization.LocalizedText = viewModel.LocalizedText;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Localization updated: LocalizationId={Id}", id);

        if (localization is not null)
        {
            await InvalidateEntityCacheAsync(localization.EntityTypeId, localization.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("بومی‌سازی با موفقیت به‌روزرسانی شد.");
        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Delete (hard delete — Localization has no IsDeleted)
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var localization = await _db.Localizations
            .FirstOrDefaultAsync(l => l.LocalizationId == id, cancellationToken);

        if (localization is null)
        {
            SetErrorMessage("بومی‌سازی یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        _db.Localizations.Remove(localization);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Localization deleted: LocalizationId={Id}", id);

        if (localization is not null)
        {
            await InvalidateEntityCacheAsync(localization.EntityTypeId, localization.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("بومی‌سازی با موفقیت حذف شد.");

        return RedirectToAction(nameof(Index));
    }
}
