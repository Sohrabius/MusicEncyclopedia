using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Services.Validators;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin track management controller (spec 10.6).
/// Route: /admin/tracks
/// Requires the CanManageTracks permission policy.
/// </summary>
[Route("/admin/tracks")]
[Authorize(Policy = PermissionConstants.CanManageTracks)]
public sealed class TracksController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<TracksController> _logger;

    public TracksController(AppDbContext db, ILogger<TracksController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  List
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var query = _db.Tracks
            .Include(t => t.AlbumTracks)
                .ThenInclude(at => at.Album)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.OriginalTitle!.ToLower().Contains(term) ||
                t.EnglishTitle!.ToLower().Contains(term) ||
                t.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var tracks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = tracks.Select(t => new TrackListItemDto
        {
            TrackId = t.TrackId,
            Slug = t.Slug,
            Title = t.Title,
            OriginalTitle = t.OriginalTitle,
            EnglishTitle = t.EnglishTitle,
            DurationSeconds = t.DurationSeconds,
            AlbumTitle = t.AlbumTracks.FirstOrDefault()?.Album?.Title
        }).ToList();

        var viewModel = new TrackListViewModel
        {
            Items = PagedResult<TrackListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Tracks";
        ViewData["ActiveMenu"] = "Tracks";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Create
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new TrackEditViewModel
        {
            LyricsAvailabilityTypes = await _db.LyricsAvailabilityTypes
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken),
            VocalStyles = await _db.VocalStyles
                .OrderBy(v => v.Name)
                .ToListAsync(cancellationToken),
            MusicalKeys = await _db.MusicalKeys
                .OrderBy(m => m.Name)
                .ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Track";
        ViewData["ActiveMenu"] = "Tracks";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        TrackEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        // Validate with FluentValidation
        var validator = new TrackValidator();
        var validationResult = await validator.ValidateAsync(
            new TrackFormModel
            {
                Title = viewModel.Title,
                Isrc = viewModel.ISRC,
                Bpm = viewModel.BPM,
                DurationSeconds = viewModel.DurationSeconds
            },
            cancellationToken);

        if (!validationResult.IsValid || !ModelState.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            viewModel.LyricsAvailabilityTypes = await _db.LyricsAvailabilityTypes
                .OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.VocalStyles = await _db.VocalStyles
                .OrderBy(v => v.Name).ToListAsync(cancellationToken);
            viewModel.MusicalKeys = await _db.MusicalKeys
                .OrderBy(m => m.Name).ToListAsync(cancellationToken);

            ViewData["Title"] = "Create Track";
            return View(viewModel);
        }

        // Create the base Entity first
        var entity = new Entity
        {
            EntityTypeId = 2, // Track entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // Create the Track
        var track = new Track
        {
            EntityId = entity.EntityId,
            Title = viewModel.Title,
            TitleSort = viewModel.TitleSort,
            OriginalTitle = viewModel.OriginalTitle,
            EnglishTitle = viewModel.EnglishTitle,
            DurationSeconds = viewModel.DurationSeconds,
            ReleaseDate = viewModel.ReleaseDate,
            ReleaseDatePrecision = viewModel.ReleaseDatePrecision,
            RecordingStartDate = viewModel.RecordingStartDate,
            RecordingEndDate = viewModel.RecordingEndDate,
            RecordingDatePrecision = viewModel.RecordingDatePrecision,
            Description = viewModel.Description,
            LyricsAvailabilityTypeId = viewModel.LyricsAvailabilityTypeId,
            VocalStyleId = viewModel.VocalStyleId,
            MusicalKeyId = viewModel.MusicalKeyId,
            BPM = viewModel.BPM,
            ISRC = viewModel.ISRC,
            IsInstrumental = viewModel.IsInstrumental,
            IsExplicit = viewModel.IsExplicit,
            CopyrightNotice = viewModel.CopyrightNotice,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Tracks.Add(track);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Track created: TrackId={TrackId}, Title={Title}, Slug={Slug}",
            track.TrackId, track.Title, track.Slug);

        await InvalidateEntityCacheAsync("Track", track.TrackId, "Created");

        SetSuccessMessage($"Track \"{track.Title}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = track.TrackId });
    }

    // ──────────────────────────────────────────────
    //  Edit
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var track = await _db.Tracks
            .FirstOrDefaultAsync(t => t.TrackId == id, cancellationToken);

        if (track is null)
        {
            _logger.LogWarning("Track not found for edit: TrackId={TrackId}", id);
            SetErrorMessage("Track not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new TrackEditViewModel
        {
            TrackId = track.TrackId,
            Title = track.Title,
            TitleSort = track.TitleSort,
            OriginalTitle = track.OriginalTitle,
            EnglishTitle = track.EnglishTitle,
            DurationSeconds = track.DurationSeconds,
            ReleaseDate = track.ReleaseDate,
            ReleaseDatePrecision = track.ReleaseDatePrecision,
            RecordingStartDate = track.RecordingStartDate,
            RecordingEndDate = track.RecordingEndDate,
            RecordingDatePrecision = track.RecordingDatePrecision,
            Description = track.Description,
            LyricsAvailabilityTypeId = track.LyricsAvailabilityTypeId,
            VocalStyleId = track.VocalStyleId,
            MusicalKeyId = track.MusicalKeyId,
            BPM = track.BPM,
            ISRC = track.ISRC,
            IsInstrumental = track.IsInstrumental,
            IsExplicit = track.IsExplicit,
            CopyrightNotice = track.CopyrightNotice,
            Slug = track.Slug,
            RowVersion = track.RowVersion,
            LyricsAvailabilityTypes = await _db.LyricsAvailabilityTypes
                .OrderBy(l => l.Name).ToListAsync(cancellationToken),
            VocalStyles = await _db.VocalStyles
                .OrderBy(v => v.Name).ToListAsync(cancellationToken),
            MusicalKeys = await _db.MusicalKeys
                .OrderBy(m => m.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {track.Title}";
        ViewData["ActiveMenu"] = "Tracks";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        TrackEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.TrackId)
        {
            SetErrorMessage("Track ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        // Validate with FluentValidation
        var validator = new TrackValidator();
        var validationResult = await validator.ValidateAsync(
            new TrackFormModel
            {
                Title = viewModel.Title,
                Isrc = viewModel.ISRC,
                Bpm = viewModel.BPM,
                DurationSeconds = viewModel.DurationSeconds
            },
            cancellationToken);

        if (!validationResult.IsValid || !ModelState.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            viewModel.LyricsAvailabilityTypes = await _db.LyricsAvailabilityTypes
                .OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.VocalStyles = await _db.VocalStyles
                .OrderBy(v => v.Name).ToListAsync(cancellationToken);
            viewModel.MusicalKeys = await _db.MusicalKeys
                .OrderBy(m => m.Name).ToListAsync(cancellationToken);

            ViewData["Title"] = $"Edit: {viewModel.Title}";
            return View(viewModel);
        }

        var track = await _db.Tracks
            .FirstOrDefaultAsync(t => t.TrackId == id, cancellationToken);

        if (track is null)
        {
            _logger.LogWarning("Track not found for update: TrackId={TrackId}", id);
            SetErrorMessage("Track not found. It may have been deleted.");
            return RedirectToAction(nameof(Index));
        }

        // Concurrency check
        if (viewModel.RowVersion is not null)
        {
            _db.Entry(track).Property(nameof(Track.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        // Update fields
        track.Title = viewModel.Title;
        track.TitleSort = viewModel.TitleSort;
        track.OriginalTitle = viewModel.OriginalTitle;
        track.EnglishTitle = viewModel.EnglishTitle;
        track.DurationSeconds = viewModel.DurationSeconds;
        track.ReleaseDate = viewModel.ReleaseDate;
        track.ReleaseDatePrecision = viewModel.ReleaseDatePrecision;
        track.RecordingStartDate = viewModel.RecordingStartDate;
        track.RecordingEndDate = viewModel.RecordingEndDate;
        track.RecordingDatePrecision = viewModel.RecordingDatePrecision;
        track.Description = viewModel.Description;
        track.LyricsAvailabilityTypeId = viewModel.LyricsAvailabilityTypeId;
        track.VocalStyleId = viewModel.VocalStyleId;
        track.MusicalKeyId = viewModel.MusicalKeyId;
        track.BPM = viewModel.BPM;
        track.ISRC = viewModel.ISRC;
        track.IsInstrumental = viewModel.IsInstrumental;
        track.IsExplicit = viewModel.IsExplicit;
        track.CopyrightNotice = viewModel.CopyrightNotice;
        track.Slug = viewModel.Slug;
        track.ModifiedBy = User.Identity?.Name ?? "system";
        track.ModifiedAt = DateTime.UtcNow;

        // Also update the Entity slug
        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == track.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Track updated: TrackId={TrackId}, Title={Title}", track.TrackId, track.Title);

            await InvalidateEntityCacheAsync("Track", track.TrackId, "Updated");

            SetSuccessMessage($"Track \"{track.Title}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating track {TrackId}", id);
            SetErrorMessage("This track was modified by another user. Please reload and try again.");

            viewModel.LyricsAvailabilityTypes = await _db.LyricsAvailabilityTypes
                .OrderBy(l => l.Name).ToListAsync(cancellationToken);
            viewModel.VocalStyles = await _db.VocalStyles
                .OrderBy(v => v.Name).ToListAsync(cancellationToken);
            viewModel.MusicalKeys = await _db.MusicalKeys
                .OrderBy(m => m.Name).ToListAsync(cancellationToken);
            viewModel.RowVersion = track.RowVersion;

            return View(viewModel);
        }

        return RedirectToAction(nameof(Edit), new { id });
    }

    // ──────────────────────────────────────────────
    //  Soft Delete
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var track = await _db.Tracks
            .FirstOrDefaultAsync(t => t.TrackId == id, cancellationToken);

        if (track is null)
        {
            SetErrorMessage("Track not found.");
            return RedirectToAction(nameof(Index));
        }

        track.IsDeleted = true;
        track.ModifiedBy = User.Identity?.Name ?? "system";
        track.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == track.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Track soft-deleted: TrackId={TrackId}, Title={Title}", id, track.Title);

        await InvalidateEntityCacheAsync("Track", id, "Deleted");

        SetSuccessMessage($"Track \"{track.Title}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Restore
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        var track = await _db.Tracks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.TrackId == id, cancellationToken);

        if (track is null)
        {
            SetErrorMessage("Track not found.");
            return RedirectToAction(nameof(Index));
        }

        track.IsDeleted = false;
        track.ModifiedBy = User.Identity?.Name ?? "system";
        track.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == track.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Track restored: TrackId={TrackId}, Title={Title}", id, track.Title);

        await InvalidateEntityCacheAsync("Track", id, "Restored");

        SetSuccessMessage($"Track \"{track.Title}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
