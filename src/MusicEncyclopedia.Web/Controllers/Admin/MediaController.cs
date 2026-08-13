using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Services.Infrastructure;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin media management controller (spec 10.11).
/// Route: /admin/media
/// Requires the CanManageMedia permission policy.
/// </summary>
[Route("/admin/media")]
[Authorize(Policy = PermissionConstants.CanManageMedia)]
public sealed class MediaController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<MediaController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly FileValidationService _fileValidation;

    public MediaController(
        AppDbContext db,
        ILogger<MediaController> logger,
        IWebHostEnvironment env,
        FileValidationService fileValidation)
    {
        _db = db;
        _logger = logger;
        _env = env;
        _fileValidation = fileValidation;
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
        string? mediaType = null,
        int? entityTypeId = null,
        int? entityId = null,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 20;

        var query = _db.Media
            .Include(m => m.MediaType)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLower();
            query = query.Where(m =>
                m.FileName.ToLower().Contains(term) ||
                (m.Url != null && m.Url.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(mediaType))
        {
            query = query.Where(m => m.MediaType != null && m.MediaType.Code == mediaType);
        }

        if (entityTypeId.HasValue && entityId.HasValue)
        {
            // Filter by assigned entity
            var assignedMediaIds = _db.MediaAssignments
                .Where(ma => ma.EntityTypeId == entityTypeId.Value && ma.EntityId == entityId.Value)
                .Select(ma => ma.MediaId);
            query = query.Where(m => assignedMediaIds.Contains(m.MediaId));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var mediaItems = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Get assignment counts
        var mediaIds = mediaItems.Select(m => m.MediaId).ToList();
        var assignmentCounts = await _db.MediaAssignments
            .Where(ma => mediaIds.Contains(ma.MediaId))
            .GroupBy(ma => ma.MediaId)
            .Select(g => new { MediaId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.MediaId, x => x.Count, cancellationToken);

        var items = mediaItems.Select(m => new MediaListItemDto
        {
            MediaId = m.MediaId,
            FileName = m.FileName,
            MediaTypeName = m.MediaType?.Name,
            MediaTypeCode = m.MediaType?.Code,
            FileSize = m.FileSize,
            FormattedFileSize = FormatFileSize(m.FileSize),
            Width = m.Width,
            Height = m.Height,
            ThumbnailUrl150 = m.ThumbnailUrl150,
            AssignmentCount = assignmentCounts.TryGetValue(m.MediaId, out var cnt) ? cnt : 0,
            CreatedAt = m.CreatedAt
        }).ToList();

        var viewModel = new MediaListViewModel
        {
            Items = PagedResult<MediaListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems),
            SearchQuery = q,
            MediaTypeFilter = mediaType,
            AssignedEntityTypeId = entityTypeId,
            AssignedEntityId = entityId,
            Page = page,
            PageSize = pageSize,
            MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken),
            EntityTypes = await _db.EntityTypes.OrderBy(et => et.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Media Manager";
        ViewData["ActiveMenu"] = "Media";

        return View(viewModel);
    }

    // ──────────────────────────────────────────────
    //  Upload
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("Upload")]
    public async Task<IActionResult> Upload(CancellationToken cancellationToken = default)
    {
        var viewModel = new MediaUploadViewModel
        {
            MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = "Upload Media";
        ViewData["ActiveMenu"] = "Media";

        return View(viewModel);
    }

    [HttpPost]
    [Route("Upload")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        MediaUploadViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            viewModel.MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Upload Media";
            return View(viewModel);
        }

        var file = viewModel.File;
        if (file is null || file.Length == 0)
        {
            ModelState.AddModelError(nameof(viewModel.File), "Please select a file to upload.");
            viewModel.MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Upload Media";
            return View(viewModel);
        }

        // Determine media type code from explicit selection or infer from MIME
        var mediaTypeCode = await GetMediaTypeCode(viewModel.MediaTypeId, file.ContentType, cancellationToken);
        if (mediaTypeCode is null)
        {
            ModelState.AddModelError(nameof(viewModel.MediaTypeId), "Could not determine the media type for this file. Please select a media type.");
            viewModel.MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Upload Media";
            return View(viewModel);
        }

        // Validate file through centralized FileValidationService
        var (isValid, errorMessage) = _fileValidation.Validate(file, mediaTypeCode);
        if (!isValid)
        {
            ModelState.AddModelError(nameof(viewModel.File), errorMessage!);
            viewModel.MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = "Upload Media";
            return View(viewModel);
        }

        // Build file path
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "media");
        Directory.CreateDirectory(uploadsDir);

        var displayFileName = string.IsNullOrWhiteSpace(viewModel.FileName)
            ? file.FileName
            : viewModel.FileName;

        // Generate a safe, unique file name for storage (prevents path traversal and collisions)
        var uniqueFileName = FileValidationService.GenerateSafeFileName(file.FileName);
        var filePath = Path.Combine(uploadsDir, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        // Get image dimensions if applicable
        int? width = null;
        int? height = null;
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) && file.ContentType != "image/svg+xml")
        {
            try
            {
                // Use a simple approach — in production you'd use ImageSharp or similar
                // For now we leave width/height null for the user to fill in
            }
            catch
            {
                // Ignore errors getting dimensions
            }
        }

        var relativePath = $"/uploads/media/{uniqueFileName}";

        var media = new MusicEncyclopedia.Data.Entities.Media
        {
            FileName = displayFileName,
            FilePath = relativePath,
            MediaTypeId = viewModel.MediaTypeId ?? await InferMediaTypeId(file.ContentType, cancellationToken),
            Url = relativePath,
            Width = width,
            Height = height,
            FileSize = file.Length,
            MimeType = file.ContentType,
            CreatedBy = User.Identity?.Name ?? "system",
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        _db.Media.Add(media);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media uploaded: MediaId={MediaId}, FileName={FileName}, Size={Size}, MimeType={MimeType}",
            media.MediaId, media.FileName, media.FileSize, media.MimeType);

        await InvalidateEntityCacheAsync("Media", media.MediaId, "Created");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage($"فایل «{media.FileName}» با موفقیت بارگذاری شد.");
        return RedirectToAction(nameof(Edit), new { id = media.MediaId });
    }

    // ──────────────────────────────────────────────
    //  Edit (metadata)
    // ──────────────────────────────────────────────

    [HttpGet]
    [Route("{id:int}/Edit")]
    public async Task<IActionResult> Edit(
        int id,
        CancellationToken cancellationToken = default)
    {
        var media = await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == id, cancellationToken);

        if (media is null)
        {
            SetErrorMessage("رسانه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new MediaEditViewModel
        {
            MediaId = media.MediaId,
            FileName = media.FileName,
            MediaTypeId = media.MediaTypeId,
            Url = media.Url,
            Width = media.Width,
            Height = media.Height,
            FileSize = media.FileSize,
            MimeType = media.MimeType,
            FilePath = media.FilePath,
            ThumbnailUrl150 = media.ThumbnailUrl150,
            ThumbnailUrl300 = media.ThumbnailUrl300,
            ThumbnailUrl600 = media.ThumbnailUrl600,
            ThumbnailUrl1200 = media.ThumbnailUrl1200,
            MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken)
        };

        ViewData["Title"] = $"Edit: {media.FileName}";
        ViewData["ActiveMenu"] = "Media";

        return View(viewModel);
    }

    [HttpPost]
    [Route("{id:int}/Edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        MediaEditViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (id != viewModel.MediaId)
        {
            SetErrorMessage("شناسه رسانه ناسازگار است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            viewModel.MediaTypes = await _db.MediaTypes.OrderBy(mt => mt.Name).ToListAsync(cancellationToken);
            ViewData["Title"] = $"Edit: {viewModel.FileName}";
            return View(viewModel);
        }

        var media = await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == id, cancellationToken);

        if (media is null)
        {
            SetErrorMessage("رسانه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        media.FileName = viewModel.FileName;
        media.MediaTypeId = viewModel.MediaTypeId;
        media.Url = viewModel.Url;
        media.Width = viewModel.Width;
        media.Height = viewModel.Height;
        media.FileSize = viewModel.FileSize;
        media.MimeType = viewModel.MimeType;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media updated: MediaId={MediaId}, FileName={FileName}", media.MediaId, media.FileName);

        await InvalidateEntityCacheAsync("Media", media.MediaId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage($"رسانه «{media.FileName}» با موفقیت به‌روزرسانی شد.");

        return RedirectToAction(nameof(Edit), new { id });
    }

    // ──────────────────────────────────────────────
    //  Replace File
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/ReplaceFile")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReplaceFile(
        int id,
        IFormFile? file,
        CancellationToken cancellationToken = default)
    {
        var media = await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == id, cancellationToken);

        if (media is null)
        {
            SetErrorMessage("رسانه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        if (file is null || file.Length == 0)
        {
            SetErrorMessage("لطفاً فایلی را برای جایگزینی انتخاب کنید.");
            return RedirectToAction(nameof(Edit), new { id });
        }

        // Infer media type code from the existing media record
        var mediaTypeCode = FileValidationService.InferMediaTypeCode(media.MimeType)
                            ?? "IMAGE"; // fallback — shouldn't happen

        // Validate the replacement file via centralized service
        var (isValid, errorMessage) = _fileValidation.Validate(file, mediaTypeCode);
        if (!isValid)
        {
            SetErrorMessage(errorMessage!);
            return RedirectToAction(nameof(Edit), new { id });
        }

        // Delete the old file
        var oldFilePath = Path.Combine(_env.WebRootPath, media.FilePath.TrimStart('/'));
        if (System.IO.File.Exists(oldFilePath))
        {
            System.IO.File.Delete(oldFilePath);
            _logger.LogInformation("Deleted old media file: {FilePath}", oldFilePath);
        }

        // Save the new file with a safe unique name
        var uploadsDir = Path.Combine(_env.WebRootPath, "uploads", "media");
        Directory.CreateDirectory(uploadsDir);
        var uniqueFileName = FileValidationService.GenerateSafeFileName(file.FileName);
        var newFilePath = Path.Combine(uploadsDir, uniqueFileName);

        await using (var stream = new FileStream(newFilePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativePath = $"/uploads/media/{uniqueFileName}";

        media.FilePath = relativePath;
        media.Url = relativePath;
        media.FileSize = file.Length;
        media.MimeType = file.ContentType;
        media.FileName = file.FileName;

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media file replaced: MediaId={MediaId}, NewFile={FileName}", media.MediaId, file.FileName);

        await InvalidateEntityCacheAsync("Media", media.MediaId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage($"فایل با موفقیت برای «{media.FileName}».");

        return RedirectToAction(nameof(Edit), new { id });
    }

    // ──────────────────────────────────────────────
    //  Delete (soft delete)
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("{id:int}/Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int id,
        CancellationToken cancellationToken = default)
    {
        var media = await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == id, cancellationToken);

        if (media is null)
        {
            SetErrorMessage("رسانه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        media.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media soft-deleted: MediaId={MediaId}, FileName={FileName}", id, media.FileName);

        await InvalidateEntityCacheAsync("Media", media.MediaId, "Deleted");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage($"رسانه «{media.FileName}» به‌صورت نرم حذف شد.");

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
        var media = await _db.Media
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(m => m.MediaId == id, cancellationToken);

        if (media is null)
        {
            SetErrorMessage("رسانه یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        media.IsDeleted = false;
        await _db.SaveChangesAsync(cancellationToken);

        await InvalidateEntityCacheAsync("Media", media.MediaId, "Restored");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage($"رسانه «{media.FileName}» بازیابی شد.");
        return RedirectToAction(nameof(Index));
    }

    // ──────────────────────────────────────────────
    //  Assign to Entity
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("Assign")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
        MediaAssignViewModel viewModel,
        CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            SetErrorMessage("داده‌های تخصیص نامعتبر است.");
            return RedirectToAction(nameof(Edit), new { id = viewModel.MediaId });
        }

        // Check if assignment already exists
        var existingAssignment = await _db.MediaAssignments
            .FirstOrDefaultAsync(ma =>
                ma.MediaId == viewModel.MediaId &&
                ma.EntityTypeId == viewModel.EntityTypeId &&
                ma.EntityId == viewModel.EntityId &&
                ma.MediaRoleTypeId == viewModel.MediaRoleTypeId,
                cancellationToken);

        if (existingAssignment is not null)
        {
            // Update existing assignment
            existingAssignment.IsPrimary = viewModel.IsPrimary;
            existingAssignment.DisplayOrder = viewModel.DisplayOrder;
        }
        else
        {
            // If this is set as primary, unset any existing primary assignments for this entity
            if (viewModel.IsPrimary)
            {
                var existingPrimary = await _db.MediaAssignments
                    .Where(ma =>
                        ma.EntityTypeId == viewModel.EntityTypeId &&
                        ma.EntityId == viewModel.EntityId &&
                        ma.IsPrimary)
                    .ToListAsync(cancellationToken);
                foreach (var ep in existingPrimary)
                {
                    ep.IsPrimary = false;
                }
            }

            var assignment = new MediaAssignment
            {
                MediaId = viewModel.MediaId,
                EntityTypeId = viewModel.EntityTypeId,
                EntityId = viewModel.EntityId,
                MediaRoleTypeId = viewModel.MediaRoleTypeId,
                DisplayOrder = viewModel.DisplayOrder,
                IsPrimary = viewModel.IsPrimary
            };

            _db.MediaAssignments.Add(assignment);
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media assigned: MediaId={MediaId}, EntityTypeId={EtId}, EntityId={EId}",
            viewModel.MediaId, viewModel.EntityTypeId, viewModel.EntityId);

        await InvalidateEntityCacheAsync(viewModel.EntityTypeId, viewModel.EntityId, "Updated");
        await InvalidateEntityCacheAsync("Media", viewModel.MediaId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("رسانه با موفقیت به موجودیت تخصیص یافت.");
        return RedirectToAction(nameof(Edit), new { id = viewModel.MediaId });
    }

    // ──────────────────────────────────────────────
    //  Remove Assignment
    // ──────────────────────────────────────────────

    [HttpPost]
    [Route("RemoveAssignment/{assignmentId:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAssignment(
        int assignmentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await _db.MediaAssignments
            .FirstOrDefaultAsync(ma => ma.MediaAssignmentId == assignmentId, cancellationToken);

        if (assignment is null)
        {
            SetErrorMessage("تخصیص یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        var mediaId = assignment.MediaId;
        _db.MediaAssignments.Remove(assignment);
        await _db.SaveChangesAsync(cancellationToken);

        if (assignment is not null)
        {
            await InvalidateEntityCacheAsync(assignment.EntityTypeId, assignment.EntityId, "Updated");
        }
        await InvalidateEntityCacheAsync("Media", mediaId, "Updated");
        await InvalidateBroadCacheAsync();

        SetSuccessMessage("تخصیص رسانه حذف شد.");
        return RedirectToAction(nameof(Edit), new { id = mediaId });
    }

    // ──────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Formats a byte count into a human-readable string (B, KB, MB, GB).
    /// </summary>
    private static string FormatFileSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MB",
            _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB"
        };
    }

    /// <summary>
    /// Resolves a media type code (IMAGE, AUDIO, VIDEO, PDF) from an optional explicit
    /// MediaTypeId or by inferring from the MIME type. Used before file validation
    /// so the <see cref="FileValidationService"/> knows which MIME types are allowed.
    /// </summary>
    private async Task<string?> GetMediaTypeCode(int? mediaTypeId, string mimeType, CancellationToken cancellationToken)
    {
        if (mediaTypeId.HasValue)
        {
            var mt = await _db.MediaTypes.FindAsync([mediaTypeId.Value], cancellationToken);
            return mt?.Code;
        }

        return FileValidationService.InferMediaTypeCode(mimeType);
    }

    /// <summary>
    /// Infers the <see cref="MediaType"/> database ID from a MIME type string.
    /// </summary>
    private async Task<int?> InferMediaTypeId(string mimeType, CancellationToken cancellationToken)
    {
        var code = FileValidationService.InferMediaTypeCode(mimeType);
        if (code is null)
            return null;

        var mediaType = await _db.MediaTypes
            .FirstOrDefaultAsync(mt => mt.Code == code, cancellationToken);

        return mediaType?.MediaTypeId;
    }
}
