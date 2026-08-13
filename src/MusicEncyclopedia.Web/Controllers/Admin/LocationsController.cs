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
/// Admin location management controller.
/// Route: /admin/locations
/// Requires the CanManageLocations permission policy.
/// </summary>
[Route("/admin/locations")]
[Authorize(Policy = PermissionConstants.CanManageLocations)]
public sealed class LocationsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<LocationsController> _logger;

    public LocationsController(AppDbContext db, ILogger<LocationsController> logger)
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

        var query = _db.Locations
            .Include(l => l.LocationType)
            .Include(l => l.ParentLocation)
            .Include(l => l.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(l =>
                l.Name.ToLower().Contains(term) ||
                l.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var locations = await query
            .OrderBy(l => l.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = locations.Select(l => new LocationListItemDto
        {
            LocationId = l.LocationId,
            Name = l.Name,
            Slug = l.Slug,
            LocationTypeName = l.LocationType?.Name,
            ParentLocationName = l.ParentLocation?.Name,
            CountryName = l.Country?.Name
        }).ToList();

        var viewModel = new LocationListViewModel
        {
            Items = PagedResult<LocationListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Locations";
        ViewData["ActiveMenu"] = "Locations";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new LocationEditViewModel
        {
            LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken),
            ParentLocations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Location";
        ViewData["ActiveMenu"] = "Locations";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        LocationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            viewModel.ParentLocations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Location";
            return View(viewModel);
        }

        var slugExists = await _db.Locations.AnyAsync(l => l.Slug == viewModel.Slug, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A location with this slug already exists.");
            viewModel.LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            viewModel.ParentLocations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Location";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 13, // Location entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var location = new Location
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            LocationTypeId = viewModel.LocationTypeId,
            ParentLocationId = viewModel.ParentLocationId,
            CountryId = viewModel.CountryId,
            Latitude = viewModel.Latitude,
            Longitude = viewModel.Longitude,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Locations.Add(location);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Location created: LocationId={Id}, Name={Name}, Slug={Slug}",
            location.LocationId, location.Name, location.Slug);

        await InvalidateEntityCacheAsync("Location", location.LocationId, "Created");

        SetSuccessMessage($"مکان «{location.Name}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = location.LocationId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var location = await _db.Locations
            .FirstOrDefaultAsync(l => l.LocationId == id, cancellationToken);

        if (location is null)
        {
            SetErrorMessage("مکان یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new LocationEditViewModel
        {
            LocationId = location.LocationId,
            Name = location.Name,
            LocationTypeId = location.LocationTypeId,
            ParentLocationId = location.ParentLocationId,
            CountryId = location.CountryId,
            Latitude = location.Latitude,
            Longitude = location.Longitude,
            Slug = location.Slug,
            RowVersion = location.RowVersion,
            LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken),
            ParentLocations = await _db.Locations.Where(l => !l.IsDeleted && l.LocationId != id).OrderBy(l => l.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {location.Name}";
        ViewData["ActiveMenu"] = "Locations";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        LocationEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.LocationId)
        {
            SetErrorMessage("شناسه مکان ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            viewModel.ParentLocations = await _db.Locations.Where(l => !l.IsDeleted && l.LocationId != id).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var location = await _db.Locations
            .FirstOrDefaultAsync(l => l.LocationId == id, cancellationToken);

        if (location is null)
        {
            SetErrorMessage("مکان یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var slugExists = await _db.Locations.AnyAsync(l => l.Slug == viewModel.Slug && l.LocationId != id, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A location with this slug already exists.");
            viewModel.LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            viewModel.ParentLocations = await _db.Locations.Where(l => !l.IsDeleted && l.LocationId != id).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        if (viewModel.RowVersion is not null)
        {
            _db.Entry(location).Property(nameof(Location.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        location.Name = viewModel.Name;
        location.LocationTypeId = viewModel.LocationTypeId;
        location.ParentLocationId = viewModel.ParentLocationId;
        location.CountryId = viewModel.CountryId;
        location.Latitude = viewModel.Latitude;
        location.Longitude = viewModel.Longitude;
        location.Slug = viewModel.Slug;
        location.ModifiedBy = User.Identity?.Name ?? "system";
        location.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == location.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Location updated: LocationId={Id}, Name={Name}", location.LocationId, location.Name);

            await InvalidateEntityCacheAsync("Location", location.LocationId, "Updated");

            SetSuccessMessage($"مکان «{location.Name}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating location {Id}", id);
            SetErrorMessage("این مکان توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.RowVersion = location.RowVersion;
            viewModel.LocationTypes = await _db.LocationTypes.OrderBy(lt => lt.Name).ToListAsync(cancellationToken);
            viewModel.ParentLocations = await _db.Locations.Where(l => !l.IsDeleted && l.LocationId != id).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            return View(viewModel);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var location = await _db.Locations
            .FirstOrDefaultAsync(l => l.LocationId == id, cancellationToken);

        if (location is null)
        {
            SetErrorMessage("مکان یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        location.IsDeleted = true;
        location.ModifiedBy = User.Identity?.Name ?? "system";
        location.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == location.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Location soft-deleted: LocationId={Id}, Name={Name}", id, location.Name);

        await InvalidateEntityCacheAsync("Location", location.LocationId, "Deleted");

        SetSuccessMessage($"مکان «{location.Name}» حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var location = await _db.Locations
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.LocationId == id, cancellationToken);

        if (location is null)
        {
            SetErrorMessage("مکان یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        location.IsDeleted = false;
        location.ModifiedBy = User.Identity?.Name ?? "system";
        location.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == location.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Location", location.LocationId, "Restored");

        SetSuccessMessage($"مکان «{location.Name}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
