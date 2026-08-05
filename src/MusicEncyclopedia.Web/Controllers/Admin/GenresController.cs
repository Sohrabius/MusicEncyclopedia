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
/// Admin genre management controller.
/// Route: /admin/genres
/// Requires the CanManageGenres permission policy.
/// </summary>
[Route("/admin/genres")]
[Authorize(Policy = PermissionConstants.CanManageGenres)]
public sealed class GenresController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<GenresController> _logger;

    public GenresController(AppDbContext db, ILogger<GenresController> logger)
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

        var query = _db.Genres
            .Include(g => g.ParentGenre)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(g =>
                g.Name.ToLower().Contains(term) ||
                g.Slug.ToLower().Contains(term));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var genres = await query
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = genres.Select(g => new GenreListItemDto
        {
            GenreId = g.GenreId,
            Slug = g.Slug,
            Name = g.Name,
            ParentGenreName = g.ParentGenre?.Name
        }).ToList();

        var viewModel = new GenreListViewModel
        {
            Items = PagedResult<GenreListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            Page = page,
            PageSize = pageSize
        };

        ViewData["Title"] = "Genres";
        ViewData["ActiveMenu"] = "Genres";

        return View(viewModel);
    }

    [HttpGet]
    [Route("Create")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken = default)
    {
        var viewModel = new GenreEditViewModel
        {
            ParentGenres = await _db.Genres.OrderBy(g => g.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Create Genre";
        ViewData["ActiveMenu"] = "Genres";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        GenreEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.ParentGenres = await _db.Genres.OrderBy(g => g.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Create Genre";
            return View(viewModel);
        }

        var entity = new Entity
        {
            EntityTypeId = 5, // Genre entity type
            Slug = viewModel.Slug,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Set<Entity>().Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var genre = new Genre
        {
            EntityId = entity.EntityId,
            Name = viewModel.Name,
            NameSort = viewModel.NameSort,
            Description = viewModel.Description,
            ParentGenreId = viewModel.ParentGenreId,
            Slug = viewModel.Slug,
            IsDeleted = false,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow
        };
        _db.Genres.Add(genre);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Genre created: GenreId={GenreId}, Name={Name}", genre.GenreId, genre.Name);

        await InvalidateEntityCacheAsync("Genre", genre.GenreId, "Created");

        SetSuccessMessage($"Genre \"{genre.Name}\" created successfully.");
        return RedirectToAction(nameof(Edit), new { id = genre.GenreId });
    }

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken = default)
    {
        var genre = await _db.Genres
            .FirstOrDefaultAsync(g => g.GenreId == id, cancellationToken);

        if (genre is null)
        {
            SetErrorMessage("Genre not found.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new GenreEditViewModel
        {
            GenreId = genre.GenreId,
            Name = genre.Name,
            NameSort = genre.NameSort,
            Description = genre.Description,
            ParentGenreId = genre.ParentGenreId,
            Slug = genre.Slug,
            RowVersion = genre.RowVersion,
            ParentGenres = await _db.Genres
                .Where(g => g.GenreId != id)
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {genre.Name}";
        ViewData["ActiveMenu"] = "Genres";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        GenreEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.GenreId)
        {
            SetErrorMessage("Genre ID mismatch.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.ParentGenres = await _db.Genres
                .Where(g => g.GenreId != id)
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.Name}";
            return View(viewModel);
        }

        var genre = await _db.Genres
            .FirstOrDefaultAsync(g => g.GenreId == id, cancellationToken);

        if (genre is null)
        {
            SetErrorMessage("Genre not found.");
            return RedirectToAction(nameof(Index));
        }

        if (viewModel.RowVersion is not null)
            _db.Entry(genre).Property(nameof(Genre.RowVersion)).OriginalValue = viewModel.RowVersion;

        genre.Name = viewModel.Name;
        genre.NameSort = viewModel.NameSort;
        genre.Description = viewModel.Description;
        genre.ParentGenreId = viewModel.ParentGenreId;
        genre.Slug = viewModel.Slug;
        genre.ModifiedBy = User.Identity?.Name ?? "system";
        genre.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>()
            .FirstOrDefaultAsync(e => e.EntityId == genre.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.Slug = viewModel.Slug;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        try
        {
            await _db.SaveChangesAsync(cancellationToken);

            await InvalidateEntityCacheAsync("Genre", genre.GenreId, "Updated");

            SetSuccessMessage($"Genre \"{genre.Name}\" updated successfully.");
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict updating genre {GenreId}", id);
            SetErrorMessage("This genre was modified by another user. Please reload and try again.");
            viewModel.ParentGenres = await _db.Genres
                .Where(g => g.GenreId != id)
                .OrderBy(g => g.Name)
                .ToListAsync(cancellationToken);
            viewModel.RowVersion = genre.RowVersion;
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
        var genre = await _db.Genres.FirstOrDefaultAsync(g => g.GenreId == id, cancellationToken);
        if (genre is null)
        {
            SetErrorMessage("Genre not found.");
            return RedirectToAction(nameof(Index));
        }

        genre.IsDeleted = true;
        genre.ModifiedBy = User.Identity?.Name ?? "system";
        genre.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().FirstOrDefaultAsync(e => e.EntityId == genre.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = true;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Genre", genre.GenreId, "Deleted");

        SetSuccessMessage($"Genre \"{genre.Name}\" has been deleted (soft).");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Route("{id:int}/Restore")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Restore(int id, CancellationToken cancellationToken = default)
    {
        var genre = await _db.Genres.IgnoreQueryFilters()
            .FirstOrDefaultAsync(g => g.GenreId == id, cancellationToken);
        if (genre is null)
        {
            SetErrorMessage("Genre not found.");
            return RedirectToAction(nameof(Index));
        }

        genre.IsDeleted = false;
        genre.ModifiedBy = User.Identity?.Name ?? "system";
        genre.ModifiedAt = DateTime.UtcNow;

        var entity = await _db.Set<Entity>().IgnoreQueryFilters()
            .FirstOrDefaultAsync(e => e.EntityId == genre.EntityId, cancellationToken);
        if (entity is not null)
        {
            entity.IsDeleted = false;
            entity.ModifiedBy = User.Identity?.Name ?? "system";
            entity.ModifiedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Genre", genre.GenreId, "Restored");

        SetSuccessMessage($"Genre \"{genre.Name}\" has been restored.");
        return RedirectToAction(nameof(Index));
    }
}
