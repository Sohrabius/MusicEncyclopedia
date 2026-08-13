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
/// Admin performance event management controller.
/// Route: /admin/events
/// Requires the CanManageEvents permission policy.
/// </summary>
[Route("/admin/events")]
[Authorize(Policy = PermissionConstants.CanManageEvents)]
public sealed class EventsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<EventsController> _logger;

    public EventsController(AppDbContext db, ILogger<EventsController> logger)
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

        var query = _db.PerformanceEvents
            .Include(pe => pe.EventType)
            .Include(pe => pe.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(pe =>
                pe.Slug.ToLower().Contains(term) ||
                (pe.PerformanceNotes != null && pe.PerformanceNotes.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var events = await query
            .OrderByDescending(pe => pe.Date)
            .ThenByDescending(pe => pe.PerformanceEventId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = events.Select(pe => new EventListItemDto
        {
            PerformanceEventId = pe.PerformanceEventId,
            Slug = pe.Slug,
            EventTypeName = pe.EventType?.Name,
            Date = pe.Date,
            VenueName = pe.Location?.Name
        }).ToList();

        var viewModel = new EventListViewModel
        {
            Items = PagedResult<EventListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Performance Events";
        ViewData["ActiveMenu"] = "Performance Events";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new EventEditViewModel
        {
            EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken),
            Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Performance Event";
        ViewData["ActiveMenu"] = "Performance Events";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        EventEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Performance Event";
            return View(viewModel);
        }

        var slugExists = await _db.PerformanceEvents.AnyAsync(pe => pe.Slug == viewModel.Slug, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "An event with this slug already exists.");
            viewModel.EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Performance Event";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 12, // PerformanceEvent entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var evt = new PerformanceEvent
        {
            EntityId = entity.EntityId,
            EventTypeId = viewModel.EventTypeId,
            LocationId = viewModel.LocationId,
            Date = viewModel.Date,
            AudienceInfo = viewModel.AudienceInfo,
            PerformanceNotes = viewModel.PerformanceNotes,
            ImprovisationNotes = viewModel.ImprovisationNotes,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.PerformanceEvents.Add(evt);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Performance event created: PerformanceEventId={Id}, Slug={Slug}",
            evt.PerformanceEventId, evt.Slug);

        await InvalidateEntityCacheAsync("PerformanceEvent", evt.PerformanceEventId, "Created");

        SetSuccessMessage($"رویداد اجرا «{evt.Slug}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = evt.PerformanceEventId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evt = await _db.PerformanceEvents
            .FirstOrDefaultAsync(pe => pe.PerformanceEventId == id, cancellationToken);

        if (evt is null)
        {
            SetErrorMessage("رویداد اجرا یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new EventEditViewModel
        {
            PerformanceEventId = evt.PerformanceEventId,
            EventTypeId = evt.EventTypeId,
            LocationId = evt.LocationId,
            Date = evt.Date,
            AudienceInfo = evt.AudienceInfo,
            PerformanceNotes = evt.PerformanceNotes,
            ImprovisationNotes = evt.ImprovisationNotes,
            Slug = evt.Slug,
            RowVersion = evt.RowVersion,
            EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken),
            Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {evt.Slug}";
        ViewData["ActiveMenu"] = "Performance Events";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        EventEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.PerformanceEventId)
        {
            SetErrorMessage("شناسه رویداد ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Slug}";
            return View(viewModel);
        }

        var evt = await _db.PerformanceEvents
            .FirstOrDefaultAsync(pe => pe.PerformanceEventId == id, cancellationToken);

        if (evt is null)
        {
            SetErrorMessage("رویداد اجرا یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var slugExists = await _db.PerformanceEvents.AnyAsync(pe => pe.Slug == viewModel.Slug && pe.PerformanceEventId != id, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "An event with this slug already exists.");
            viewModel.EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Slug}";
            return View(viewModel);
        }

        if (viewModel.RowVersion is not null)
        {
            _db.Entry(evt).Property(nameof(PerformanceEvent.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        evt.EventTypeId = viewModel.EventTypeId;
        evt.LocationId = viewModel.LocationId;
        evt.Date = viewModel.Date;
        evt.AudienceInfo = viewModel.AudienceInfo;
        evt.PerformanceNotes = viewModel.PerformanceNotes;
        evt.ImprovisationNotes = viewModel.ImprovisationNotes;
        evt.Slug = viewModel.Slug;
        evt.ModifiedBy = User.Identity?.Name ?? "system";
        evt.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == evt.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Performance event updated: PerformanceEventId={Id}", evt.PerformanceEventId);

            await InvalidateEntityCacheAsync("PerformanceEvent", evt.PerformanceEventId, "Updated");

            SetSuccessMessage($"رویداد اجرا «{evt.Slug}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating performance event {Id}", id);
            SetErrorMessage("این رویداد توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.RowVersion = evt.RowVersion;
            viewModel.EventTypes = await _db.EventTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
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
        var evt = await _db.PerformanceEvents
            .FirstOrDefaultAsync(pe => pe.PerformanceEventId == id, cancellationToken);

        if (evt is null)
        {
            SetErrorMessage("رویداد اجرا یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        evt.IsDeleted = true;
        evt.ModifiedBy = User.Identity?.Name ?? "system";
        evt.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == evt.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Performance event soft-deleted: PerformanceEventId={Id}", id);

        await InvalidateEntityCacheAsync("PerformanceEvent", evt.PerformanceEventId, "Deleted");

        SetSuccessMessage($"رویداد اجرا «{evt.Slug}» حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var evt = await _db.PerformanceEvents
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(pe => pe.PerformanceEventId == id, cancellationToken);

        if (evt is null)
        {
            SetErrorMessage("رویداد اجرا یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        evt.IsDeleted = false;
        evt.ModifiedBy = User.Identity?.Name ?? "system";
        evt.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == evt.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("PerformanceEvent", evt.PerformanceEventId, "Restored");

        SetSuccessMessage($"رویداد اجرا «{evt.Slug}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
