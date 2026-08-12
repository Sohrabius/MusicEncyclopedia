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
/// Admin source management controller.
/// Route: /admin/sources
/// Sources are the references cited on entity pages (books, articles, websites...).
/// </summary>
[Route("/admin/sources")]
[Authorize(Policy = PermissionConstants.CanManageCitations)]
public sealed class SourcesController : AdminBaseController
{
    private const int SourceEntityTypeId = 17;

    private readonly AppDbContext _db;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(AppDbContext db, ILogger<SourcesController> logger)
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

        var query = _db.Sources
            .Include(s => s.SourceType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(s =>
                s.Title.ToLower().Contains(term) ||
                s.Slug.ToLower().Contains(term) ||
                (s.Author != null && s.Author.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var sources = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = sources.Select(s => new SourceListItem
        {
            SourceId = s.SourceId,
            Slug = s.Slug,
            Title = s.Title,
            TypeName = s.SourceType?.Name,
            Author = s.Author
        }).ToList();

        var viewModel = new SourceListViewModel
        {
            Items = PagedResult<SourceListItem>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Sources";
        ViewData["ActiveMenu"] = "Sources";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new SourceEditViewModel
        {
            SourceTypes = await _db.SourceTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken),
            Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Source";
        ViewData["ActiveMenu"] = "Sources";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SourceEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.SourceTypes = await _db.SourceTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Source";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = SourceEntityTypeId,
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var source = new Source
        {
            EntityId = entity.EntityId,
            SourceTypeId = viewModel.SourceTypeId,
            Title = viewModel.Title,
            Author = viewModel.Author,
            PublisherId = viewModel.PublisherId,
            PublicationDate = viewModel.PublicationDate,
            Url = viewModel.Url,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Sources.Add(source);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Source created: SourceId={SourceId}, Title={Title}", source.SourceId, source.Title);

        await InvalidateEntityCacheAsync("Source", source.SourceId, "Created");

        SetSuccessMessage($"Source \"{source.Title}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = source.SourceId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var source = await _db.Sources
            .FirstOrDefaultAsync(s => s.SourceId == id, cancellationToken);

        if (source is null)
        {
            SetErrorMessage("Source not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new SourceEditViewModel
        {
            SourceId = source.SourceId,
            Title = source.Title,
            SourceTypeId = source.SourceTypeId,
            Author = source.Author,
            PublisherId = source.PublisherId,
            PublicationDate = source.PublicationDate,
            Url = source.Url,
            Slug = source.Slug,
            RowVersion = source.RowVersion,
            SourceTypes = await _db.SourceTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken),
            Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {source.Title}";
        ViewData["ActiveMenu"] = "Sources";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        SourceEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.SourceId)
        {
            SetErrorMessage("Source ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.SourceTypes = await _db.SourceTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Title}";
            return View(viewModel);
        }

        var source = await _db.Sources
            .FirstOrDefaultAsync(s => s.SourceId == id, cancellationToken);

        if (source is null)
        {
            SetErrorMessage("Source not found.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(source).Property(nameof(Source.RowVersion)).OriginalValue = viewModel.RowVersion;

        source.Title = viewModel.Title;
        source.SourceTypeId = viewModel.SourceTypeId;
        source.Author = viewModel.Author;
        source.PublisherId = viewModel.PublisherId;
        source.PublicationDate = viewModel.PublicationDate;
        source.Url = viewModel.Url;
        source.Slug = viewModel.Slug;
        source.ModifiedBy = User.Identity?.Name ?? "system";
        source.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == source.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Source", source.SourceId, "Updated");

            SetSuccessMessage($"Source \"{source.Title}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating source {SourceId}", id);
            SetErrorMessage("This source was modified by another user. Please reload and try again.");
            viewModel.SourceTypes = await _db.SourceTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = source.RowVersion;
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
        var source = await _db.Sources.FirstOrDefaultAsync(s => s.SourceId == id, cancellationToken);
        if (source is null)
        {
            SetErrorMessage("Source not found.");
            return RedirectToAction(nameof(Index));
        }

        source.IsDeleted = true;
        source.ModifiedBy = User.Identity?.Name ?? "system";
        source.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == source.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Source", source.SourceId, "Deleted");

        SetSuccessMessage($"Source \"{source.Title}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var source = await _db.Sources.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.SourceId == id, cancellationToken);
        if (source is null)
        {
            SetErrorMessage("Source not found.");
            return RedirectToAction(nameof(Index));
        }

        source.IsDeleted = false;
        source.ModifiedBy = User.Identity?.Name ?? "system";
        source.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == source.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Source", source.SourceId, "Restored");

        SetSuccessMessage($"Source \"{source.Title}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
