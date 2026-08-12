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
/// Admin recording session management controller.
/// Route: /admin/sessions
/// Requires the CanManageSessions permission policy.
/// </summary>
[Route("/admin/sessions")]
[Authorize(Policy = PermissionConstants.CanManageSessions)]
public sealed class SessionsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(AppDbContext db, ILogger<SessionsController> logger)
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

        var query = _db.RecordingSessions
            .Include(rs => rs.SessionType)
            .Include(rs => rs.Location)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(rs =>
                rs.Slug.ToLower().Contains(term) ||
                (rs.Notes != null && rs.Notes.ToLower().Contains(term)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var sessions = await query
            .OrderByDescending(rs => rs.StartDate)
            .ThenByDescending(rs => rs.RecordingSessionId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = sessions.Select(rs => new SessionListItemDto
        {
            RecordingSessionId = rs.RecordingSessionId,
            Slug = rs.Slug,
            SessionTypeName = rs.SessionType?.Name,
            StartDate = rs.StartDate,
            EndDate = rs.EndDate,
            LocationName = rs.Location?.Name
        }).ToList();

        var viewModel = new SessionListViewModel
        {
            Items = PagedResult<SessionListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Recording Sessions";
        ViewData["ActiveMenu"] = "Recording Sessions";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new SessionEditViewModel
        {
            SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken),
            Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Recording Session";
        ViewData["ActiveMenu"] = "Recording Sessions";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SessionEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Recording Session";
            return View(viewModel);
        }

        // Check for duplicate slug
        var slugExists = await _db.RecordingSessions.AnyAsync(rs => rs.Slug == viewModel.Slug, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A session with this slug already exists.");
            viewModel.SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Recording Session";
            return View(viewModel);
        }

        // Create the base Entity first
        var entity = new Entity
        {
            EntityTypeId = 11, // RecordingSession entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var session = new RecordingSession
        {
            EntityId = entity.EntityId,
            SessionTypeId = viewModel.SessionTypeId,
            LocationId = viewModel.LocationId,
            StartDate = viewModel.StartDate,
            EndDate = viewModel.EndDate,
            DatePrecision = viewModel.DatePrecision,
            Notes = viewModel.Notes,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.RecordingSessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recording session created: RecordingSessionId={Id}, Slug={Slug}",
            session.RecordingSessionId, session.Slug);

        await InvalidateEntityCacheAsync("RecordingSession", session.RecordingSessionId, "Created");

        SetSuccessMessage($"جلسه ضبط «{session.Slug}» با موفقیت ایجاد شد.");
        return RedirectToAction(nameof(Edit), new { id = session.RecordingSessionId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var session = await _db.RecordingSessions
            .FirstOrDefaultAsync(rs => rs.RecordingSessionId == id, cancellationToken);

        if (session is null)
        {
            SetErrorMessage("جلسه ضبط یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new SessionEditViewModel
        {
            RecordingSessionId = session.RecordingSessionId,
            SessionTypeId = session.SessionTypeId,
            LocationId = session.LocationId,
            StartDate = session.StartDate,
            EndDate = session.EndDate,
            DatePrecision = session.DatePrecision,
            Notes = session.Notes,
            Slug = session.Slug,
            RowVersion = session.RowVersion,
            SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken),
            Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {session.Slug}";
        ViewData["ActiveMenu"] = "Recording Sessions";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        SessionEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.RecordingSessionId)
        {
            SetErrorMessage("شناسه جلسه ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Slug}";
            return View(viewModel);
        }

        var session = await _db.RecordingSessions
            .FirstOrDefaultAsync(rs => rs.RecordingSessionId == id, cancellationToken);

        if (session is null)
        {
            SetErrorMessage("جلسه ضبط یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        // Check for duplicate slug (excluding current)
        var slugExists = await _db.RecordingSessions.AnyAsync(rs => rs.Slug == viewModel.Slug && rs.RecordingSessionId != id, cancellationToken);
        if (slugExists)
        {
            ModelState.AddModelError(nameof(viewModel.Slug), "A session with this slug already exists.");
            viewModel.SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken);
            viewModel.Locations = await _db.Locations.Where(l => !l.IsDeleted).OrderBy(l => l.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Slug}";
            return View(viewModel);
        }

        if (viewModel.RowVersion is not null)
        {
            _db.Entry(session).Property(nameof(RecordingSession.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        session.SessionTypeId = viewModel.SessionTypeId;
        session.LocationId = viewModel.LocationId;
        session.StartDate = viewModel.StartDate;
        session.EndDate = viewModel.EndDate;
        session.DatePrecision = viewModel.DatePrecision;
        session.Notes = viewModel.Notes;
        session.Slug = viewModel.Slug;
        session.ModifiedBy = User.Identity?.Name ?? "system";
        session.ModifiedAt = DateTime.UtcNow;

        // Update Entity slug
        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == session.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Recording session updated: RecordingSessionId={Id}", session.RecordingSessionId);

            await InvalidateEntityCacheAsync("RecordingSession", session.RecordingSessionId, "Updated");

            SetSuccessMessage($"جلسه ضبط «{session.Slug}» با موفقیت به‌روزرسانی شد.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating recording session {Id}", id);
            SetErrorMessage("این جلسه توسط کاربر دیگری تغییر کرده است. لطفاً دوباره بارگذاری و تلاش کنید.");
            viewModel.RowVersion = session.RowVersion;
            viewModel.SessionTypes = await _db.SessionTypes.OrderBy(st => st.Name).ToListAsync(cancellationToken);
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
        var session = await _db.RecordingSessions
            .FirstOrDefaultAsync(rs => rs.RecordingSessionId == id, cancellationToken);

        if (session is null)
        {
            SetErrorMessage("جلسه ضبط یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        session.IsDeleted = true;
        session.ModifiedBy = User.Identity?.Name ?? "system";
        session.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == session.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Recording session soft-deleted: RecordingSessionId={Id}", id);

        await InvalidateEntityCacheAsync("RecordingSession", session.RecordingSessionId, "Deleted");

        SetSuccessMessage($"جلسه ضبط «{session.Slug}» حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var session = await _db.RecordingSessions
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(rs => rs.RecordingSessionId == id, cancellationToken);

        if (session is null)
        {
            SetErrorMessage("جلسه ضبط یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        session.IsDeleted = false;
        session.ModifiedBy = User.Identity?.Name ?? "system";
        session.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == session.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("RecordingSession", session.RecordingSessionId, "Restored");

        SetSuccessMessage($"جلسه ضبط «{session.Slug}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }
}
