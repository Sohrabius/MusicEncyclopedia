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
/// Admin certification management controller.
/// Route: /admin/certifications
/// Certifications are industry awards/recognitions (gold, platinum, ...).
/// </summary>
[Route("/admin/certifications")]
[Authorize(Policy = PermissionConstants.CanManageAwards)]
public sealed class CertificationsController : AdminBaseController
{
    private const int CertificationEntityTypeId = 15;

    private readonly AppDbContext _db;
    private readonly ILogger<CertificationsController> _logger;

    public CertificationsController(AppDbContext db, ILogger<CertificationsController> logger)
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

        var query = _db.Certifications
            .Include(c => c.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Slug.ToLower().Contains(term) ||
                (c.Organization != null && c.Organization.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var certifications = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = certifications.Select(c => new CertificationListItem
        {
            CertificationId = c.CertificationId,
            Slug = c.Slug,
            Name = c.Name,
            Organization = c.Organization,
            CountryName = c.Country?.Name
        }).ToList();

        var viewModel = new CertificationListViewModel
        {
            Items = PagedResult<CertificationListItem>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Certifications";
        ViewData["ActiveMenu"] = "Certifications";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new CertificationEditViewModel
        {
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Certification";
        ViewData["ActiveMenu"] = "Certifications";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CertificationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Certification";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = CertificationEntityTypeId,
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var certification = new Certification
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            Organization = viewModel.Organization,
            CountryId = viewModel.CountryId,
            Description = viewModel.Description,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Certifications.Add(certification);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Certification created: CertificationId={CertificationId}, Name={Name}", certification.CertificationId, certification.Name);

        await InvalidateEntityCacheAsync("Certification", certification.CertificationId, "Created");

        SetSuccessMessage($"Certification \"{certification.Name}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = certification.CertificationId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var certification = await _db.Certifications
            .FirstOrDefaultAsync(c => c.CertificationId == id, cancellationToken);

        if (certification is null)
        {
            SetErrorMessage("Certification not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new CertificationEditViewModel
        {
            CertificationId = certification.CertificationId,
            Name = certification.Name,
            Organization = certification.Organization,
            CountryId = certification.CountryId,
            Description = certification.Description,
            Slug = certification.Slug,
            RowVersion = certification.RowVersion,
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {certification.Name}";
        ViewData["ActiveMenu"] = "Certifications";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        CertificationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.CertificationId)
        {
            SetErrorMessage("Certification ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var certification = await _db.Certifications
            .FirstOrDefaultAsync(c => c.CertificationId == id, cancellationToken);

        if (certification is null)
        {
            SetErrorMessage("Certification not found.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(certification).Property(nameof(Certification.RowVersion)).OriginalValue = viewModel.RowVersion;

        certification.Name = viewModel.Name;
        certification.Organization = viewModel.Organization;
        certification.CountryId = viewModel.CountryId;
        certification.Description = viewModel.Description;
        certification.Slug = viewModel.Slug;
        certification.ModifiedBy = User.Identity?.Name ?? "system";
        certification.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == certification.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Certification", certification.CertificationId, "Updated");

            SetSuccessMessage($"Certification \"{certification.Name}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating certification {CertificationId}", id);
            SetErrorMessage("This certification was modified by another user. Please reload and try again.");
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = certification.RowVersion;
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
        var certification = await _db.Certifications.FirstOrDefaultAsync(c => c.CertificationId == id, cancellationToken);
        if (certification is null)
        {
            SetErrorMessage("Certification not found.");
            return RedirectToAction(nameof(Index));
        }

        certification.IsDeleted = true;
        certification.ModifiedBy = User.Identity?.Name ?? "system";
        certification.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == certification.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Certification", certification.CertificationId, "Deleted");

        SetSuccessMessage($"Certification \"{certification.Name}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var certification = await _db.Certifications.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.CertificationId == id, cancellationToken);
        if (certification is null)
        {
            SetErrorMessage("Certification not found.");
            return RedirectToAction(nameof(Index));
        }

        certification.IsDeleted = false;
        certification.ModifiedBy = User.Identity?.Name ?? "system";
        certification.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == certification.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Certification", certification.CertificationId, "Restored");

        SetSuccessMessage($"Certification \"{certification.Name}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
