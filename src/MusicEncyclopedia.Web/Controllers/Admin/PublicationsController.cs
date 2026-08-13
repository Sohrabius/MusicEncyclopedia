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
/// Admin publication management controller.
/// Route: /admin/publications
/// Publications are person-authored works (books, liner notes, interviews...).
/// </summary>
[Route("/admin/publications")]
[Authorize(Policy = PermissionConstants.CanManagePeople)]
public sealed class PublicationsController : AdminBaseController
{
    private const int PublicationEntityTypeId = 10;

    private readonly AppDbContext _db;
    private readonly ILogger<PublicationsController> _logger;

    public PublicationsController(AppDbContext db, ILogger<PublicationsController> logger)
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

        var query = _db.Publications
            .Include(p => p.PublicationType)
            .Include(p => p.Person)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                p.Slug.ToLower().Contains(term) ||
                (p.ISBN != null && p.ISBN.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var publications = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = publications.Select(p => new PublicationListItem
        {
            PublicationId = p.PublicationId,
            Slug = p.Slug,
            Title = p.Title,
            TypeName = p.PublicationType?.Name,
            PersonName = p.Person.FullName,
            PublicationDate = p.PublicationDate
        }).ToList();

        var viewModel = new PublicationListViewModel
        {
            Items = PagedResult<PublicationListItem>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Publications";
        ViewData["ActiveMenu"] = "Publications";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new PublicationEditViewModel
        {
            PublicationTypes = await _db.PublicationTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken),
            People = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken),
            Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Publication";
        ViewData["ActiveMenu"] = "Publications";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PublicationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.PublicationTypes = await _db.PublicationTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.People = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken);
            viewModel.Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Publication";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = PublicationEntityTypeId,
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var publication = new Publication
        {
            EntityId = entity.EntityId,
            PersonId = viewModel.PersonId,
            Title = viewModel.Title,
            PublicationTypeId = viewModel.PublicationTypeId,
            PublisherId = viewModel.PublisherId,
            PublicationDate = viewModel.PublicationDate,
            PublicationDatePrecision = viewModel.PublicationDatePrecision,
            ISBN = viewModel.ISBN,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Publications.Add(publication);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Publication created: PublicationId={PublicationId}, Title={Title}", publication.PublicationId, publication.Title);

        await InvalidateEntityCacheAsync("Publication", publication.PublicationId, "Created");
        await InvalidateEntityCacheAsync(EntityTypeConstants.Person, publication.PersonId, "Updated");

        SetSuccessMessage($"انتشارات «{publication.Title}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = publication.PublicationId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var publication = await _db.Publications
            .FirstOrDefaultAsync(p => p.PublicationId == id, cancellationToken);

        if (publication is null)
        {
            SetErrorMessage("انتشارات یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new PublicationEditViewModel
        {
            PublicationId = publication.PublicationId,
            Title = publication.Title,
            PublicationTypeId = publication.PublicationTypeId,
            PersonId = publication.PersonId,
            PublisherId = publication.PublisherId,
            PublicationDate = publication.PublicationDate,
            PublicationDatePrecision = publication.PublicationDatePrecision,
            ISBN = publication.ISBN,
            Slug = publication.Slug,
            RowVersion = publication.RowVersion,
            PublicationTypes = await _db.PublicationTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken),
            People = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken),
            Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {publication.Title}";
        ViewData["ActiveMenu"] = "Publications";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PublicationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.PublicationId)
        {
            SetErrorMessage("شناسه انتشارات ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.PublicationTypes = await _db.PublicationTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.People = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken);
            viewModel.Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Title}";
            return View(viewModel);
        }

        var publication = await _db.Publications
            .FirstOrDefaultAsync(p => p.PublicationId == id, cancellationToken);

        if (publication is null)
        {
            SetErrorMessage("انتشارات یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(publication).Property(nameof(Publication.RowVersion)).OriginalValue = viewModel.RowVersion;

        publication.Title = viewModel.Title;
        publication.PublicationTypeId = viewModel.PublicationTypeId;
        publication.PersonId = viewModel.PersonId;
        publication.PublisherId = viewModel.PublisherId;
        publication.PublicationDate = viewModel.PublicationDate;
        publication.PublicationDatePrecision = viewModel.PublicationDatePrecision;
        publication.ISBN = viewModel.ISBN;
        publication.Slug = viewModel.Slug;
        publication.ModifiedBy = User.Identity?.Name ?? "system";
        publication.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == publication.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Publication", publication.PublicationId, "Updated");
            await InvalidateEntityCacheAsync(EntityTypeConstants.Person, publication.PersonId, "Updated");

            SetSuccessMessage($"انتشارات «{publication.Title}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating publication {PublicationId}", id);
            SetErrorMessage("این انتشارات توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.PublicationTypes = await _db.PublicationTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.People = await _db.People.OrderBy(p => p.FullName).ToListAsync(cancellationToken);
            viewModel.Companies = await _db.Companies.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = publication.RowVersion;
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
        var publication = await _db.Publications.FirstOrDefaultAsync(p => p.PublicationId == id, cancellationToken);
        if (publication is null)
        {
            SetErrorMessage("انتشارات یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        publication.IsDeleted = true;
        publication.ModifiedBy = User.Identity?.Name ?? "system";
        publication.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == publication.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Publication", publication.PublicationId, "Deleted");
        await InvalidateEntityCacheAsync(EntityTypeConstants.Person, publication.PersonId, "Updated");

        SetSuccessMessage($"انتشارات «{publication.Title}» به‌صورت نرم حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var publication = await _db.Publications.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PublicationId == id, cancellationToken);
        if (publication is null)
        {
            SetErrorMessage("انتشارات یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        publication.IsDeleted = false;
        publication.ModifiedBy = User.Identity?.Name ?? "system";
        publication.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == publication.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Publication", publication.PublicationId, "Restored");
        await InvalidateEntityCacheAsync(EntityTypeConstants.Person, publication.PersonId, "Updated");

        SetSuccessMessage($"انتشارات «{publication.Title}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
