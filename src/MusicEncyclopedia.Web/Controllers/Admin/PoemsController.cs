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
/// Admin poem management controller (spec 10.9).
/// Route: /admin/poems
/// Requires the CanManagePoems permission policy.
/// </summary>
[Route("/admin/poems")]
[Authorize(Policy = PermissionConstants.CanManagePoems)]
public sealed class PoemsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<PoemsController> _logger;

    public PoemsController(AppDbContext db, ILogger<PoemsController> logger)
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

        var query = _db.Poems
            .Include(p => p.Person)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.OriginalTitle!.ToLower().Contains(term) ||
                p.EnglishTitle!.ToLower().Contains(term) ||
                p.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var poems = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = poems.Select(p => new PoemListItemDto
        {
            PoemId = p.PoemId,
            Slug = p.Slug,
            Title = p.Title,
            OriginalTitle = p.OriginalTitle,
            EnglishTitle = p.EnglishTitle,
            PoetName = p.Person?.FullName,
            OriginalPublicationDate = p.OriginalPublicationDate
        }).ToList();

        var viewModel = new PoemListViewModel
        {
            Items = PagedResult<PoemListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Poems";
        ViewData["ActiveMenu"] = "Poems";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new PoemEditViewModel
        {
            Poets = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken),
            Publications = await _db.Publications.OrderBy(p => p.Title).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Poem";
        ViewData["ActiveMenu"] = "Poems";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PoemEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Poets = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken);
            viewModel.Publications = await _db.Publications.OrderBy(p => p.Title).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Poem";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 8, // Poem entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var poem = new Poem
        {
            EntityId = entity.EntityId,
            Title = viewModel.Title,
            OriginalTitle = viewModel.OriginalTitle,
            EnglishTitle = viewModel.EnglishTitle,
            PersonId = viewModel.PoetId,
            PublicationId = viewModel.PublicationId,
            Source = viewModel.Source,
            Book = viewModel.Book,
            OriginalPublicationDate = viewModel.OriginalPublicationDate,
            OriginalPublicationDatePrecision = viewModel.OriginalPublicationDatePrecision,
            ExternalReferenceUrl = viewModel.ExternalReferenceUrl,
            Copyright = viewModel.Copyright,
            Notes = viewModel.Notes,
            CanonicalText = viewModel.CanonicalText,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Poems.Add(poem);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Poem created: PoemId={PoemId}, Title={Title}", poem.PoemId, poem.Title);

        await InvalidateEntityCacheAsync("Poem", poem.PoemId, "Created");

        SetSuccessMessage($"Poem \"{poem.Title}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = poem.PoemId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var poem = await _db.Poems
            .FirstOrDefaultAsync(p => p.PoemId == id, cancellationToken);

        if (poem is null)
        {
            SetErrorMessage("Poem not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new PoemEditViewModel
        {
            PoemId = poem.PoemId,
            Title = poem.Title,
            OriginalTitle = poem.OriginalTitle,
            EnglishTitle = poem.EnglishTitle,
            PoetId = poem.PersonId,
            PublicationId = poem.PublicationId,
            Source = poem.Source,
            Book = poem.Book,
            OriginalPublicationDate = poem.OriginalPublicationDate,
            OriginalPublicationDatePrecision = poem.OriginalPublicationDatePrecision,
            ExternalReferenceUrl = poem.ExternalReferenceUrl,
            Copyright = poem.Copyright,
            Notes = poem.Notes,
            CanonicalText = poem.CanonicalText,
            Slug = poem.Slug,
            RowVersion = poem.RowVersion,
            Poets = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken),
            Publications = await _db.Publications.OrderBy(p => p.Title).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {poem.Title}";
        ViewData["ActiveMenu"] = "Poems";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PoemEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.PoemId)
        {
            SetErrorMessage("Poem ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.Poets = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken);
            viewModel.Publications = await _db.Publications.OrderBy(p => p.Title).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Title}";
            return View(viewModel);
        }

        var poem = await _db.Poems
            .FirstOrDefaultAsync(p => p.PoemId == id, cancellationToken);

        if (poem is null)
        {
            SetErrorMessage("Poem not found.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(poem).Property(nameof(Poem.RowVersion)).OriginalValue = viewModel.RowVersion;

        poem.Title = viewModel.Title;
        poem.OriginalTitle = viewModel.OriginalTitle;
        poem.EnglishTitle = viewModel.EnglishTitle;
        poem.PersonId = viewModel.PoetId;
        poem.PublicationId = viewModel.PublicationId;
        poem.Source = viewModel.Source;
        poem.Book = viewModel.Book;
        poem.OriginalPublicationDate = viewModel.OriginalPublicationDate;
        poem.OriginalPublicationDatePrecision = viewModel.OriginalPublicationDatePrecision;
        poem.ExternalReferenceUrl = viewModel.ExternalReferenceUrl;
        poem.Copyright = viewModel.Copyright;
        poem.Notes = viewModel.Notes;
        poem.CanonicalText = viewModel.CanonicalText;
        poem.Slug = viewModel.Slug;
        poem.ModifiedBy = User.Identity?.Name ?? "system";
        poem.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == poem.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Poem", poem.PoemId, "Updated");

            SetSuccessMessage($"Poem \"{poem.Title}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating poem {PoemId}", id);
            SetErrorMessage("This poem was modified by another user. Please reload and try again.");
            viewModel.Poets = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken);
            viewModel.Publications = await _db.Publications.OrderBy(p => p.Title).ToListAsync(cancellationToken);
            viewModel.RowVersion = poem.RowVersion;
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
        var poem = await _db.Poems.FirstOrDefaultAsync(p => p.PoemId == id, cancellationToken);
        if (poem is null)
        {
            SetErrorMessage("Poem not found.");
            return RedirectToAction(nameof(Index));
        }

        poem.IsDeleted = true;
        poem.ModifiedBy = User.Identity?.Name ?? "system";
        poem.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == poem.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Poem", poem.PoemId, "Deleted");

        SetSuccessMessage($"Poem \"{poem.Title}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var poem = await _db.Poems.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PoemId == id, cancellationToken);
        if (poem is null)
        {
            SetErrorMessage("Poem not found.");
            return RedirectToAction(nameof(Index));
        }

        poem.IsDeleted = false;
        poem.ModifiedBy = User.Identity?.Name ?? "system";
        poem.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == poem.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Poem", poem.PoemId, "Restored");

        SetSuccessMessage($"Poem \"{poem.Title}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
