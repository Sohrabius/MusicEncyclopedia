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
/// Admin citation management controller (spec 10.14).
/// Route: /admin/citations
/// Requires the CanManageCitations permission policy.
/// </summary>
[Route("/admin/citations")]
[Authorize(Policy = PermissionConstants.CanManageCitations)]
public sealed class CitationsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<CitationsController> _logger;

    public CitationsController(AppDbContext db, ILogger<CitationsController> logger)
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

        var query = _db.Citations
            .Include(c => c.Source)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c =>
                (c.Source != null && c.Source.Title.ToLower().Contains(term)) ||
                (c.Quote != null && c.Quote.ToLower().Contains(term)) ||
                (c.FieldName != null && c.FieldName.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var citations = await query
            .OrderByDescending(c => c.CitationId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = citations.Select(c => new CitationListItemDto
        {
            CitationId = c.CitationId,
            EntityTypeName = null,
            EntityId = c.EntityId,
            SourceTitle = c.Source?.Title,
            QuotePreview = c.Quote?.Length > 100
                ? c.Quote[..100] + "…"
                : c.Quote,
            FieldName = c.FieldName,
            PageNumber = c.PageNumber
        }).ToList();

        var viewModel = new CitationListViewModel
        {
            Items = PagedResult<CitationListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Citations";
        ViewData["ActiveMenu"] = "Citations";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Create
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new CitationEditViewModel
        {
            EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken),
            Sources = await _db.Sources.OrderBy(s => s.Title).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Citation";
        ViewData["ActiveMenu"] = "Citations";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CitationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Sources = await _db.Sources.OrderBy(s => s.Title).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Citation";
            return View(viewModel);
        }

        var citation = new Citation
        {
            EntityTypeId = viewModel.EntityTypeId,
            EntityId = viewModel.EntityId,
            SourceId = viewModel.SourceId,
            FieldName = viewModel.FieldName,
            Quote = viewModel.Quote,
            PageNumber = viewModel.PageNumber,
            Url = viewModel.Url,
            AccessedDate = viewModel.AccessedDate,
            Notes = viewModel.Notes,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };

        _db.Citations.Add(citation);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Citation created: CitationId={Id}, EntityTypeId={EtId}, EntityId={EId}",
            citation.CitationId, citation.EntityTypeId, citation.EntityId);

        await InvalidateEntityCacheAsync(citation.EntityTypeId, citation.EntityId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("ارجاع با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = citation.CitationId });
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
        var citation = await _db.Citations
            .Include(c => c.Source)
            .FirstOrDefaultAsync(c => c.CitationId == id, cancellationToken);

        if (citation is null)
        {
            SetErrorMessage("ارجاع یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new CitationEditViewModel
        {
            CitationId = citation.CitationId,
            EntityTypeId = citation.EntityTypeId,
            EntityId = citation.EntityId,
            SourceId = citation.SourceId,
            FieldName = citation.FieldName,
            Quote = citation.Quote,
            PageNumber = citation.PageNumber,
            Url = citation.Url,
            AccessedDate = citation.AccessedDate,
            Notes = citation.Notes,
            EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken),
            Sources = await _db.Sources.OrderBy(s => s.Title).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Edit Citation";
        ViewData["ActiveMenu"] = "Citations";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CitationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.CitationId)
        {
            SetErrorMessage("شناسه ارجاع ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Sources = await _db.Sources.OrderBy(s => s.Title).ToListAsync(cancellationToken);
            ViewData["Title"] = "Edit Citation";
            return View(viewModel);
        }

        var citation = await _db.Citations
            .FirstOrDefaultAsync(c => c.CitationId == id, cancellationToken);

        if (citation is null)
        {
            SetErrorMessage("ارجاع یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        citation.EntityTypeId = viewModel.EntityTypeId;
        citation.EntityId = viewModel.EntityId;
        citation.SourceId = viewModel.SourceId;
        citation.FieldName = viewModel.FieldName;
        citation.Quote = viewModel.Quote;
        citation.PageNumber = viewModel.PageNumber;
        citation.Url = viewModel.Url;
        citation.AccessedDate = viewModel.AccessedDate;
        citation.Notes = viewModel.Notes;
        citation.ModifiedBy = User.Identity?.Name ?? "system";
        citation.ModifiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Citation updated: CitationId={Id}", id);

        if (citation is not null)
        {
            await InvalidateEntityCacheAsync(citation.EntityTypeId, citation.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("ارجاع با موفقیت به‌روزرسانی شد.");

        return RedirectToAction(nameof(Edit), new { id });
    }

    // ──────────────────────────────────────────────
    //  Delete (hard delete — Citation has no IsDeleted)
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var citation = await _db.Citations
            .FirstOrDefaultAsync(c => c.CitationId == id, cancellationToken);

        if (citation is null)
        {
            SetErrorMessage("ارجاع یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        _db.Citations.Remove(citation);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Citation deleted: CitationId={Id}", id);

        if (citation is not null)
        {
            await InvalidateEntityCacheAsync(citation.EntityTypeId, citation.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("ارجاع با موفقیت حذف شد.");

        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Inline Source Creation (AJAX)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Returns the partial view for inline source creation (used in a modal).
    /// </summary>
    [HttpGet]
    [Route("CreateSourceInline")]
    public async Task<IActionResult> CreateSourceInline(CancellationToken cancellationToken = default)
    {
        ViewBag.SourceTypes = await _db.SourceTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken);
        return PartialView("_CreateSourceInline");
    }

    /// <summary>
    /// AJAX endpoint to create a source inline and return its ID and title.
    /// </summary>
    [HttpPost]
    [Route("CreateSourceInline")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateSourceInline(
        [FromForm] string title,
        [FromForm] int? sourceTypeId,
        [FromForm] string? author,
        [FromForm] string? url,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Json(new { success = false, message = "Title is required." });
        }

        // Generate a slug from the title
        var slug = title.ToLower()
            .Replace(" ", "-")
            .Replace("'", "")
            .Replace(".", "")
            .Replace(",", "")
            .Replace(":", "")
            .Replace(";", "")
            .Replace("(", "")
            .Replace(")", "");
        // Ensure unique slug
        var existingSlug = await _db.Sources.AnyAsync(s => s.Slug == slug, cancellationToken);
        if (existingSlug)
        {
            slug = slug + "-" + Guid.NewGuid().ToString("N")[..6];
        }

        // Create Entity for Source
        var entity = new Entity
        {
            EntityTypeId = 17, // Source entity type — adjust if dynamic lookup is needed
            Slug = slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var source = new Source
        {
            EntityId = entity.EntityId,
            SourceTypeId = sourceTypeId,
            Title = title.Trim(),
            Author = author,
            Url = url,
            Slug = slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };

        _db.Sources.Add(source);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Source created inline: SourceId={SourceId}, Title={Title}", source.SourceId, source.Title);

        await InvalidateBroadCacheAsync();

        return Json(new { success = true, sourceId = source.SourceId, title = source.Title });
    }
}
