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
/// Admin album management controller (specs 10.4, 10.5).
/// Route: /admin/albums
/// Requires the CanManageAlbums permission policy.
/// </summary>
[Route("/admin/albums")]
[Authorize(Policy = PermissionConstants.CanManageAlbums)]
public sealed class AlbumsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<AlbumsController> _logger;

    public AlbumsController(AppDbContext db, ILogger<AlbumsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  List (spec 10.4)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Paginated album list with optional search.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        // Base query — include IsDeleted so admins can see soft-deleted items
        var query = _db.Albums
            .Include(a => a.AlbumCategory)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(a =>
                a.Title.ToLower().Contains(term) ||
                a.OriginalTitle!.ToLower().Contains(term) ||
                a.EnglishTitle!.ToLower().Contains(term) ||
                a.Slug.ToLower().Contains(term));
        }

        // Get total count for pagination
        var totalItems = await query.CountAsync(cancellationToken);

        // Fetch current page
        var albums = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Map to DTOs
        var items = albums.Select(a => new AlbumListItemDto
        {
            AlbumId = a.AlbumId,
            Slug = a.Slug,
            Title = a.Title,
            OriginalTitle = a.OriginalTitle,
            EnglishTitle = a.EnglishTitle,
            CategoryName = a.AlbumCategory?.Name,
            ReleaseDate = a.ReleaseDate,
            DurationSeconds = a.DurationSeconds,
            CoverUrl = null
        }).ToList();

        var viewModel = new AlbumListViewModel
        {
            Items = PagedResult<AlbumListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Albums";
        ViewData["ActiveMenu"] = "Albums";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Create (spec 10.5)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new AlbumEditViewModel
        {
            Categories = await _db.AlbumCategories
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Album";
        ViewData["ActiveMenu"] = "Albums";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        AlbumEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        // Validate with FluentValidation
        var validator = new AlbumValidator();
        var validationResult = await validator.ValidateAsync(
            new AlbumFormModel
            {
                Title = viewModel.Title,
                Slug = viewModel.Slug,
                ReleaseDatePrecision = viewModel.ReleaseDatePrecision,
                DurationSeconds = viewModel.DurationSeconds
            },
            cancellationToken);

        if (!validationResult.IsValid || !ModelState.IsValid)
        {
            // Add FluentValidation errors to ModelState
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            viewModel.Categories = await _db.AlbumCategories
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            ViewData["Title"] = "Create Album";
            return View(viewModel);
        }

        // Create the base Entity first
        var entity = new Entity
        {
            EntityTypeId = 1, // Album entity type — adjust if dynamic lookup is needed
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // Create the Album
        var album = new Album
        {
            EntityId = entity.EntityId,
            Title = viewModel.Title,
            TitleSort = viewModel.TitleSort,
            OriginalTitle = viewModel.OriginalTitle,
            EnglishTitle = viewModel.EnglishTitle,
            AlbumCategoryId = viewModel.AlbumCategoryId,
            ReleaseDate = viewModel.ReleaseDate,
            ReleaseDatePrecision = viewModel.ReleaseDatePrecision,
            RecordingStartDate = viewModel.RecordingStartDate,
            RecordingEndDate = viewModel.RecordingEndDate,
            RecordingDatePrecision = viewModel.RecordingDatePrecision,
            Description = viewModel.Description,
            CoverMediaId = viewModel.CoverMediaId,
            DurationSeconds = viewModel.DurationSeconds,
            CopyrightNotice = viewModel.CopyrightNotice,
            Slug = viewModel.Slug,
            IsOfficial = viewModel.IsOfficial,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Albums.Add(album);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Album created: AlbumId={AlbumId}, Title={Title}, Slug={Slug}",
            album.AlbumId, album.Title, album.Slug);

        await InvalidateEntityCacheAsync("Album", album.AlbumId, "Created");

        SetSuccessMessage($"Album \"{album.Title}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = album.AlbumId });
    }

    // ──────────────────────────────────────────────
    //  Edit (spec 10.5)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var album = await _db.Albums
            .Include(a => a.AlbumCategory)
            .FirstOrDefaultAsync(a => a.AlbumId == id, cancellationToken);

        if (album is null)
        {
            _logger.LogWarning("Album not found for edit: AlbumId={AlbumId}", id);
            SetErrorMessage("Album not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new AlbumEditViewModel
        {
            AlbumId = album.AlbumId,
            Title = album.Title,
            TitleSort = album.TitleSort,
            OriginalTitle = album.OriginalTitle,
            EnglishTitle = album.EnglishTitle,
            AlbumCategoryId = album.AlbumCategoryId,
            ReleaseDate = album.ReleaseDate,
            ReleaseDatePrecision = album.ReleaseDatePrecision,
            RecordingStartDate = album.RecordingStartDate,
            RecordingEndDate = album.RecordingEndDate,
            RecordingDatePrecision = album.RecordingDatePrecision,
            Description = album.Description,
            CoverMediaId = album.CoverMediaId,
            DurationSeconds = album.DurationSeconds,
            CopyrightNotice = album.CopyrightNotice,
            Slug = album.Slug,
            IsOfficial = album.IsOfficial,
            RowVersion = album.RowVersion,
            Categories = await _db.AlbumCategories
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {album.Title}";
        ViewData["ActiveMenu"] = "Albums";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        AlbumEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.AlbumId)
        {
            SetErrorMessage("Album ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        // Validate with FluentValidation
        var validator = new AlbumValidator();
        var validationResult = await validator.ValidateAsync(
            new AlbumFormModel
            {
                Title = viewModel.Title,
                Slug = viewModel.Slug,
                ReleaseDatePrecision = viewModel.ReleaseDatePrecision,
                DurationSeconds = viewModel.DurationSeconds
            },
            cancellationToken);

        if (!validationResult.IsValid || !ModelState.IsValid)
        {
            foreach (var error in validationResult.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }

            viewModel.Categories = await _db.AlbumCategories
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);

            ViewData["Title"] = $"Edit: {viewModel.Title}";
            return View(viewModel);
        }

        // Load the album with concurrency token
        var album = await _db.Albums
            .FirstOrDefaultAsync(a => a.AlbumId == id, cancellationToken);

        if (album is null)
        {
            _logger.LogWarning("Album not found for update: AlbumId={AlbumId}", id);
            SetErrorMessage("Album not found. It may have been deleted.");
            return RedirectToAction(nameof(Index));
        }

        // Concurrency check
        if (viewModel.RowVersion is not null)
        {
            _db.Entry(album).Property(nameof(Album.RowVersion)).OriginalValue = viewModel.RowVersion;
        }

        // Update fields
        album.Title = viewModel.Title;
        album.TitleSort = viewModel.TitleSort;
        album.OriginalTitle = viewModel.OriginalTitle;
        album.EnglishTitle = viewModel.EnglishTitle;
        album.AlbumCategoryId = viewModel.AlbumCategoryId;
        album.ReleaseDate = viewModel.ReleaseDate;
        album.ReleaseDatePrecision = viewModel.ReleaseDatePrecision;
        album.RecordingStartDate = viewModel.RecordingStartDate;
        album.RecordingEndDate = viewModel.RecordingEndDate;
        album.RecordingDatePrecision = viewModel.RecordingDatePrecision;
        album.Description = viewModel.Description;
        album.CoverMediaId = viewModel.CoverMediaId;
        album.DurationSeconds = viewModel.DurationSeconds;
        album.CopyrightNotice = viewModel.CopyrightNotice;
        album.Slug = viewModel.Slug;
        album.IsOfficial = viewModel.IsOfficial;
        album.ModifiedBy = User.Identity?.Name ?? "system";
        album.ModifiedAt = DateTime.UtcNow;

        // Also update the Entity slug
        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == album.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Album updated: AlbumId={AlbumId}, Title={Title}", album.AlbumId, album.Title);

            await InvalidateEntityCacheAsync("Album", album.AlbumId, "Updated");

            SetSuccessMessage($"Album \"{album.Title}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating album {AlbumId}", id);
            SetErrorMessage("This album was modified by another user. Please reload and try again.");

            // Reload the view model with fresh data
            viewModel.Categories = await _db.AlbumCategories
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken);
            viewModel.RowVersion = album.RowVersion;

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
        var album = await _db.Albums
            .FirstOrDefaultAsync(a => a.AlbumId == id, cancellationToken);

        if (album is null)
        {
            SetErrorMessage("Album not found.");
            return RedirectToAction(nameof(Index));
        }

        album.IsDeleted = true;
        album.ModifiedBy = User.Identity?.Name ?? "system";
        album.ModifiedAt = DateTime.UtcNow;

        // Also soft-delete the base entity
        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == album.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Album soft-deleted: AlbumId={AlbumId}, Title={Title}", id, album.Title);

        await InvalidateEntityCacheAsync("Album", id, "Deleted");

        SetSuccessMessage($"Album \"{album.Title}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Restore (undo soft delete)
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(
        int id,
        CancellationToken cancellationToken = default)
    {
        // Need to ignore the global query filter to find soft-deleted items
        var album = await _db.Albums
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(a => a.AlbumId == id, cancellationToken);

        if (album is null)
        {
            SetErrorMessage("Album not found.");
            return RedirectToAction(nameof(Index));
        }

        album.IsDeleted = false;
        album.ModifiedBy = User.Identity?.Name ?? "system";
        album.ModifiedAt = DateTime.UtcNow;

        // Also restore the base entity
        var entity = await _db.Set<Entity>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == album.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Album restored: AlbumId={AlbumId}, Title={Title}", id, album.Title);

        await InvalidateEntityCacheAsync("Album", id, "Restored");

        SetSuccessMessage($"Album \"{album.Title}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
