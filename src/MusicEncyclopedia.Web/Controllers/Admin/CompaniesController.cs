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
/// Admin company management controller.
/// Route: /admin/companies
/// Requires the CanManageCompanies permission policy.
/// </summary>
[Route("/admin/companies")]
[Authorize(Policy = PermissionConstants.CanManageCompanies)]
public sealed class CompaniesController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(AppDbContext db, ILogger<CompaniesController> logger)
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

        var query = _db.Companies
            .Include(c => c.CompanyType)
            .Include(c => c.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.OriginalName!.ToLower().Contains(term) ||
                c.EnglishName!.ToLower().Contains(term) ||
                c.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var companies = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = companies.Select(c => new CompanyListItemDto
        {
            CompanyId = c.CompanyId,
            Slug = c.Slug,
            Name = c.Name,
            OriginalName = c.OriginalName,
            EnglishName = c.EnglishName,
            TypeName = c.CompanyType?.Name,
            CountryName = c.Country?.Name
        }).ToList();

        var viewModel = new CompanyListViewModel
        {
            Items = PagedResult<CompanyListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Companies";
        ViewData["ActiveMenu"] = "Companies";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new CompanyEditViewModel
        {
            CompanyTypes = await _db.CompanyTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Company";
        ViewData["ActiveMenu"] = "Companies";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CompanyEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.CompanyTypes = await _db.CompanyTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Company";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 4, // Company entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var company = new Company
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            NameSort = viewModel.NameSort,
            OriginalName = viewModel.OriginalName,
            EnglishName = viewModel.EnglishName,
            CompanyTypeId = viewModel.CompanyTypeId,
            CountryId = viewModel.CountryId,
            Website = viewModel.Website,
            History = viewModel.History,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Companies.Add(company);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Company created: CompanyId={CompanyId}, Name={Name}", company.CompanyId, company.Name);

        await InvalidateEntityCacheAsync("Company", company.CompanyId, "Created");

        SetSuccessMessage($"شرکت «{company.Name}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = company.CompanyId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);

        if (company is null)
        {
            SetErrorMessage("شرکت یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new CompanyEditViewModel
        {
            CompanyId = company.CompanyId,
            Name = company.Name,
            NameSort = company.NameSort,
            OriginalName = company.OriginalName,
            EnglishName = company.EnglishName,
            CompanyTypeId = company.CompanyTypeId,
            CountryId = company.CountryId,
            Website = company.Website,
            History = company.History,
            Slug = company.Slug,
            RowVersion = company.RowVersion,
            CompanyTypes = await _db.CompanyTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {company.Name}";
        ViewData["ActiveMenu"] = "Companies";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CompanyEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.CompanyId)
        {
            SetErrorMessage("شناسه شرکت ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.CompanyTypes = await _db.CompanyTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var company = await _db.Companies
            .FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);

        if (company is null)
        {
            SetErrorMessage("شرکت یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(company).Property(nameof(Company.RowVersion)).OriginalValue = viewModel.RowVersion;

        company.Name = viewModel.Name;
        company.NameSort = viewModel.NameSort;
        company.OriginalName = viewModel.OriginalName;
        company.EnglishName = viewModel.EnglishName;
        company.CompanyTypeId = viewModel.CompanyTypeId;
        company.CountryId = viewModel.CountryId;
        company.Website = viewModel.Website;
        company.History = viewModel.History;
        company.Slug = viewModel.Slug;
        company.ModifiedBy = User.Identity?.Name ?? "system";
        company.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == company.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Company", company.CompanyId, "Updated");

            SetSuccessMessage($"شرکت «{company.Name}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating company {CompanyId}", id);
            SetErrorMessage("این شرکت توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.CompanyTypes = await _db.CompanyTypes.OrderBy(t => t.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = company.RowVersion;
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
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);
        if (company is null)
        {
            SetErrorMessage("شرکت یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        company.IsDeleted = true;
        company.ModifiedBy = User.Identity?.Name ?? "system";
        company.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == company.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Company", company.CompanyId, "Deleted");

        SetSuccessMessage($"شرکت «{company.Name}» به‌صورت نرم حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var company = await _db.Companies.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.CompanyId == id, cancellationToken);
        if (company is null)
        {
            SetErrorMessage("شرکت یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        company.IsDeleted = false;
        company.ModifiedBy = User.Identity?.Name ?? "system";
        company.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == company.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Company", company.CompanyId, "Restored");

        SetSuccessMessage($"شرکت «{company.Name}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
