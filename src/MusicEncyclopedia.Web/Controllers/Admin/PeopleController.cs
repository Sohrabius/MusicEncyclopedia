using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin person management controller (spec 10.8).
/// Route: /admin/people
/// Requires the CanManagePeople permission policy.
/// </summary>
[Route("/admin/people")]
[Authorize(Policy = PermissionConstants.CanManagePeople)]
public sealed class PeopleController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<PeopleController> _logger;

    public PeopleController(AppDbContext db, ILogger<PeopleController> logger)
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

        var query = _db.People
            .Include(p => p.PersonKind)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(p =>
                p.FullName.ToLower().Contains(term) ||
                p.OriginalName!.ToLower().Contains(term) ||
                p.EnglishName!.ToLower().Contains(term) ||
                p.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var people = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = people.Select(p => new PersonListItemDto
        {
            PersonId = p.PersonId,
            Slug = p.Slug,
            FullName = p.FullName,
            OriginalName = p.OriginalName,
            EnglishName = p.EnglishName,
            KindName = p.PersonKind?.Name,
            BirthDate = p.BirthDate,
            DeathDate = p.DeathDate
        }).ToList();

        var viewModel = new PersonListViewModel
        {
            Items = Core.DTOs.PagedResult<PersonListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "People";
        ViewData["ActiveMenu"] = "People";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new PersonEditViewModel
        {
            PersonKinds = await _db.PersonKinds.OrderBy(k => k.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Person";
        ViewData["ActiveMenu"] = "People";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        PersonEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.PersonKinds = await _db.PersonKinds.OrderBy(k => k.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Person";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 3, // Person entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var person = new Person
        {
            EntityId = entity.EntityId,
            FullName = viewModel.FullName,
            FullNameSort = viewModel.FullNameSort,
            OriginalName = viewModel.OriginalName,
            EnglishName = viewModel.EnglishName,
            PersonKindId = viewModel.PersonKindId,
            Biography = viewModel.Biography,
            BirthDate = viewModel.BirthDate,
            BirthDatePrecision = viewModel.BirthDatePrecision,
            BirthLocationId = viewModel.BirthLocationId,
            DeathDate = viewModel.DeathDate,
            DeathDatePrecision = viewModel.DeathDatePrecision,
            DeathLocationId = viewModel.DeathLocationId,
            NationalityCountryId = viewModel.NationalityCountryId,
            ImageMediaId = viewModel.ImageMediaId,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.People.Add(person);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Person created: PersonId={PersonId}, FullName={FullName}",
            person.PersonId, person.FullName);

        await InvalidateEntityCacheAsync("Person", person.PersonId, "Created");

        SetSuccessMessage($"شخص «{person.FullName}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = person.PersonId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var person = await _db.People
            .FirstOrDefaultAsync(p => p.PersonId == id, cancellationToken);

        if (person is null)
        {
            SetErrorMessage("شخص یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new PersonEditViewModel
        {
            PersonId = person.PersonId,
            FullName = person.FullName,
            FullNameSort = person.FullNameSort,
            OriginalName = person.OriginalName,
            EnglishName = person.EnglishName,
            PersonKindId = person.PersonKindId,
            Biography = person.Biography,
            BirthDate = person.BirthDate,
            BirthDatePrecision = person.BirthDatePrecision,
            BirthLocationId = person.BirthLocationId,
            DeathDate = person.DeathDate,
            DeathDatePrecision = person.DeathDatePrecision,
            DeathLocationId = person.DeathLocationId,
            NationalityCountryId = person.NationalityCountryId,
            ImageMediaId = person.ImageMediaId,
            Slug = person.Slug,
            RowVersion = person.RowVersion,
            PersonKinds = await _db.PersonKinds.OrderBy(k => k.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {person.FullName}";
        ViewData["ActiveMenu"] = "People";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        PersonEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.PersonId)
        {
            SetErrorMessage("شناسه شخص ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.PersonKinds = await _db.PersonKinds.OrderBy(k => k.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.FullName}";
            return View(viewModel);
        }

        var person = await _db.People
            .FirstOrDefaultAsync(p => p.PersonId == id, cancellationToken);

        if (person is null)
        {
            SetErrorMessage("شخص یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(person).Property(nameof(Person.RowVersion)).OriginalValue = viewModel.RowVersion;

        person.FullName = viewModel.FullName;
        person.FullNameSort = viewModel.FullNameSort;
        person.OriginalName = viewModel.OriginalName;
        person.EnglishName = viewModel.EnglishName;
        person.PersonKindId = viewModel.PersonKindId;
        person.Biography = viewModel.Biography;
        person.BirthDate = viewModel.BirthDate;
        person.BirthDatePrecision = viewModel.BirthDatePrecision;
        person.BirthLocationId = viewModel.BirthLocationId;
        person.DeathDate = viewModel.DeathDate;
        person.DeathDatePrecision = viewModel.DeathDatePrecision;
        person.DeathLocationId = viewModel.DeathLocationId;
        person.NationalityCountryId = viewModel.NationalityCountryId;
        person.ImageMediaId = viewModel.ImageMediaId;
        person.Slug = viewModel.Slug;
        person.ModifiedBy = User.Identity?.Name ?? "system";
        person.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == person.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Person", person.PersonId, "Updated");

            SetSuccessMessage($"شخص «{person.FullName}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating person {PersonId}", id);
            SetErrorMessage("این شخص توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.PersonKinds = await _db.PersonKinds.OrderBy(k => k.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = person.RowVersion;
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
        var person = await _db.People.FirstOrDefaultAsync(p => p.PersonId == id, cancellationToken);
        if (person is null)
        {
            SetErrorMessage("شخص یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        person.IsDeleted = true;
        person.ModifiedBy = User.Identity?.Name ?? "system";
        person.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == person.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Person", person.PersonId, "Deleted");

        SetSuccessMessage($"شخص «{person.FullName}» به‌صورت نرم حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var person = await _db.People.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.PersonId == id, cancellationToken);
        if (person is null)
        {
            SetErrorMessage("شخص یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        person.IsDeleted = false;
        person.ModifiedBy = User.Identity?.Name ?? "system";
        person.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == person.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Person", person.PersonId, "Restored");

        SetSuccessMessage($"شخص «{person.FullName}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
