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
/// Admin instrument management controller.
/// Route: /admin/instruments
/// Requires the CanManageInstruments permission policy.
/// </summary>
[Route("/admin/instruments")]
[Authorize(Policy = PermissionConstants.CanManageInstruments)]
public sealed class InstrumentsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<InstrumentsController> _logger;

    public InstrumentsController(AppDbContext db, ILogger<InstrumentsController> logger)
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

        var query = _db.Instruments
            .Include(i => i.InstrumentFamily)
            .Include(i => i.Country)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(term) ||
                i.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var instruments = await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = instruments.Select(i => new InstrumentListItemDto
        {
            InstrumentId = i.InstrumentId,
            Slug = i.Slug,
            Name = i.Name,
            FamilyName = i.InstrumentFamily?.Name,
            CountryName = i.Country?.Name
        }).ToList();

        var viewModel = new InstrumentListViewModel
        {
            Items = PagedResult<InstrumentListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Instruments";
        ViewData["ActiveMenu"] = "Instruments";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new InstrumentEditViewModel
        {
            InstrumentFamilies = await _db.InstrumentFamilies.OrderBy(f => f.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Instrument";
        ViewData["ActiveMenu"] = "Instruments";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        InstrumentEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.InstrumentFamilies = await _db.InstrumentFamilies.OrderBy(f => f.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Instrument";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 7, // Instrument entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var instrument = new Instrument
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            Description = viewModel.Description,
            InstrumentFamilyId = viewModel.InstrumentFamilyId,
            CountryId = viewModel.CountryId,
            HistoricalNotes = viewModel.HistoricalNotes,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Instruments.Add(instrument);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Instrument created: InstrumentId={InstrumentId}, Name={Name}", instrument.InstrumentId, instrument.Name);

        await InvalidateEntityCacheAsync("Instrument", instrument.InstrumentId, "Created");

        SetSuccessMessage($"ساز «{instrument.Name}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = instrument.InstrumentId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(i => i.InstrumentId == id, cancellationToken);

        if (instrument is null)
        {
            SetErrorMessage("ساز یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new InstrumentEditViewModel
        {
            InstrumentId = instrument.InstrumentId,
            Name = instrument.Name,
            Description = instrument.Description,
            InstrumentFamilyId = instrument.InstrumentFamilyId,
            CountryId = instrument.CountryId,
            HistoricalNotes = instrument.HistoricalNotes,
            Slug = instrument.Slug,
            RowVersion = instrument.RowVersion,
            InstrumentFamilies = await _db.InstrumentFamilies.OrderBy(f => f.Name).ToListAsync(cancellationToken),
            Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {instrument.Name}";
        ViewData["ActiveMenu"] = "Instruments";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        InstrumentEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.InstrumentId)
        {
            SetErrorMessage("شناسه ساز ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.InstrumentFamilies = await _db.InstrumentFamilies.OrderBy(f => f.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var instrument = await _db.Instruments
            .FirstOrDefaultAsync(i => i.InstrumentId == id, cancellationToken);

        if (instrument is null)
        {
            SetErrorMessage("ساز یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(instrument).Property(nameof(Instrument.RowVersion)).OriginalValue = viewModel.RowVersion;

        instrument.Name = viewModel.Name;
        instrument.Description = viewModel.Description;
        instrument.InstrumentFamilyId = viewModel.InstrumentFamilyId;
        instrument.CountryId = viewModel.CountryId;
        instrument.HistoricalNotes = viewModel.HistoricalNotes;
        instrument.Slug = viewModel.Slug;
        instrument.ModifiedBy = User.Identity?.Name ?? "system";
        instrument.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == instrument.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Instrument", instrument.InstrumentId, "Updated");

            SetSuccessMessage($"ساز «{instrument.Name}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating instrument {InstrumentId}", id);
            SetErrorMessage("این ساز توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.InstrumentFamilies = await _db.InstrumentFamilies.OrderBy(f => f.Name).ToListAsync(cancellationToken);
            viewModel.Countries = await _db.Countries.OrderBy(c => c.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = instrument.RowVersion;
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
        var instrument = await _db.Instruments.FirstOrDefaultAsync(i => i.InstrumentId == id, cancellationToken);
        if (instrument is null)
        {
            SetErrorMessage("ساز یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        instrument.IsDeleted = true;
        instrument.ModifiedBy = User.Identity?.Name ?? "system";
        instrument.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == instrument.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Instrument", instrument.InstrumentId, "Deleted");

        SetSuccessMessage($"ساز «{instrument.Name}» به‌صورت نرم حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var instrument = await _db.Instruments.IgnoreQueryFilters()
            .FirstOrDefaultAsync(i => i.InstrumentId == id, cancellationToken);
        if (instrument is null)
        {
            SetErrorMessage("ساز یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        instrument.IsDeleted = false;
        instrument.ModifiedBy = User.Identity?.Name ?? "system";
        instrument.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == instrument.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Instrument", instrument.InstrumentId, "Restored");

        SetSuccessMessage($"ساز «{instrument.Name}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
