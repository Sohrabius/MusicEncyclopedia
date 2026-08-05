using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Media.Interfaces;
using MusicEncyclopedia.Media.Models;

namespace MusicEncyclopedia.Media.Services;

/// <summary>
/// Implements <see cref="IMediaService"/> by storing files on local disk
/// and persisting metadata via EF Core through <see cref="AppDbContext"/>.
/// </summary>
public sealed class MediaService : IMediaService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MediaService> _logger;

    // ──────────────────────────────────────────────
    //  File-size limits per spec §10.11
    // ──────────────────────────────────────────────
    private static readonly Dictionary<string, long> MaxSizes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image"] = 10L * 1024 * 1024,        //  10 MB
        ["application/pdf"] = 50L * 1024 * 1024, //  50 MB
        ["audio"] = 100L * 1024 * 1024,       // 100 MB
        ["video"] = 500L * 1024 * 1024,       // 500 MB
    };

    private static readonly HashSet<string> ImageMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp", "image/avif",
    };

    private static readonly string[] ThumbnailSizes = { "150", "300", "600", "1200" };

    public MediaService(
        AppDbContext db,
        IConfiguration configuration,
        ILogger<MediaService> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  Upload
    // ──────────────────────────────────────────────
    public async Task<MediaUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        // 1. Determine media-type category ─────────────────────
        var category = ResolveCategory(contentType);
        var mediaTypeEntity = await _db.MediaTypes
            .FirstOrDefaultAsync(mt => mt.Code == category, cancellationToken);

        if (mediaTypeEntity is null)
        {
            _logger.LogError("MediaType code '{Code}' not found in lookup table.", category);
            throw new InvalidOperationException($"Media type '{category}' is not configured in the database.");
        }

        // 2. Validate file size ────────────────────────────────
        var maxSize = GetMaxSize(category, contentType);
        if (fileStream.Length > maxSize)
        {
            _logger.LogWarning(
                "Upload rejected: {FileName} ({Size} bytes) exceeds {MaxSize} byte limit for {Category}.",
                fileName, fileStream.Length, maxSize, category);
            throw new InvalidOperationException(
                $"File size ({fileStream.Length:N0} bytes) exceeds the maximum allowed " +
                $"({maxSize:N0} bytes) for {category} files.");
        }

        // 3. Prepare storage paths ────────────────────────────
        var storagePath = ResolveStoragePath();
        var categoryDir = category.ToLowerInvariant();
        var targetDir = Path.Combine(storagePath, categoryDir);
        Directory.CreateDirectory(targetDir);

        var fileId = Guid.NewGuid().ToString("N");
        var ext = Path.GetExtension(fileName);
        var storedFileName = $"{fileId}{ext}";
        var fullPath = Path.Combine(targetDir, storedFileName);

        // 4. Save original file ───────────────────────────────
        await using (var targetStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(targetStream, cancellationToken);
        }

        _logger.LogInformation("File saved: {Path} ({Size} bytes)", fullPath, fileStream.Length);

        // 5. Generate thumbnails (images only) ────────────────
        string? thumb150 = null, thumb300 = null, thumb600 = null, thumb1200 = null;

        if (ImageMimeTypes.Contains(contentType))
        {
            (thumb150, thumb300, thumb600, thumb1200) = await GenerateThumbnailsAsync(
                fullPath, targetDir, fileId, ext, contentType, cancellationToken);
        }

        // 6. Build URL base ────────────────────────────────────
        var cdnBaseUrl = _configuration.GetValue<string>("Media:CdnBaseUrl")?.TrimEnd('/');
        var relativeDir = $"/uploads/{categoryDir}";

        string BuildUrl(string? suffix = null)
        {
            var fileNamePart = suffix is not null
                ? $"{suffix}_{fileId}{ext}"
                : storedFileName;

            var relativePath = $"{relativeDir}/{fileNamePart}";
            return cdnBaseUrl is not null
                ? $"{cdnBaseUrl}{relativePath}"
                : relativePath;
        }

        var originalUrl = BuildUrl();

        // 7. Persist entity ────────────────────────────────────
        var media = new Data.Entities.Media
        {
            FileName = fileName,
            FilePath = fullPath,
            MediaTypeId = mediaTypeEntity.MediaTypeId,
            Url = originalUrl,
            ThumbnailUrl150 = thumb150 is not null ? BuildUrl("thumb150") : null,
            ThumbnailUrl300 = thumb300 is not null ? BuildUrl("thumb300") : null,
            ThumbnailUrl600 = thumb600 is not null ? BuildUrl("thumb600") : null,
            ThumbnailUrl1200 = thumb1200 is not null ? BuildUrl("thumb1200") : null,
            Width = null,   // populated below for images
            Height = null,
            FileSize = fileStream.Length,
            MimeType = contentType,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",   // override from ambient context in real app
            IsDeleted = false,
        };

        // Try to read image dimensions (best effort)
        if (ImageMimeTypes.Contains(contentType))
        {
            var (w, h) = TryReadDimensions(fullPath);
            media.Width = w;
            media.Height = h;
        }

        _db.Media.Add(media);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media record created: Id={MediaId}, File={FileName}", media.MediaId, fileName);

        return new MediaUploadResult
        {
            MediaId = media.MediaId,
            FileName = fileName,
            Url = originalUrl,
            ThumbnailUrl150 = media.ThumbnailUrl150,
            ThumbnailUrl300 = media.ThumbnailUrl300,
            ThumbnailUrl600 = media.ThumbnailUrl600,
            ThumbnailUrl1200 = media.ThumbnailUrl1200,
            FileSize = fileStream.Length,
            MimeType = contentType,
            Width = media.Width,
            Height = media.Height,
        };
    }

    // ──────────────────────────────────────────────
    //  Delete (soft)
    // ──────────────────────────────────────────────
    public async Task DeleteAsync(int mediaId, CancellationToken cancellationToken)
    {
        var media = await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == mediaId, cancellationToken);

        if (media is null)
        {
            _logger.LogWarning("Delete requested for unknown MediaId={MediaId}.", mediaId);
            throw new KeyNotFoundException($"Media with ID {mediaId} was not found.");
        }

        // Soft delete
        media.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Media soft-deleted: Id={MediaId}, File={FileName}", mediaId, media.FileName);
    }

    // ──────────────────────────────────────────────
    //  GetUrl
    // ──────────────────────────────────────────────
    public async Task<string> GetUrlAsync(
        int mediaId,
        int? thumbnailSize = null,
        CancellationToken cancellationToken = default)
    {
        var media = await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == mediaId && !m.IsDeleted, cancellationToken);

        if (media is null)
        {
            _logger.LogWarning("GetUrl for unknown or deleted MediaId={MediaId}.", mediaId);
            throw new KeyNotFoundException($"Media with ID {mediaId} was not found or has been deleted.");
        }

        // No thumbnail requested → return original URL
        if (thumbnailSize is null)
        {
            return media.Url ?? string.Empty;
        }

        // Return the matching thumbnail URL by size
        return thumbnailSize switch
        {
            150 => media.ThumbnailUrl150 ?? media.Url ?? string.Empty,
            300 => media.ThumbnailUrl300 ?? media.Url ?? string.Empty,
            600 => media.ThumbnailUrl600 ?? media.Url ?? string.Empty,
            1200 => media.ThumbnailUrl1200 ?? media.Url ?? string.Empty,
            _ => media.Url ?? string.Empty,
        };
    }

    // ──────────────────────────────────────────────
    //  Private helpers
    // ──────────────────────────────────────────────

    /// <summary>
    /// Maps a MIME type to a media-type category code (IMAGE, PDF, AUDIO, VIDEO).
    /// </summary>
    private static string ResolveCategory(string contentType)
    {
        if (ImageMimeTypes.Contains(contentType)) return "IMAGE";
        if (contentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase)) return "PDF";
        if (contentType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase)) return "AUDIO";
        if (contentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase)) return "VIDEO";

        throw new InvalidOperationException($"Unsupported content type '{contentType}'.");
    }

    /// <summary>
    /// Returns the maximum allowed file size in bytes for the given category and specific MIME type.
    /// </summary>
    private static long GetMaxSize(string category, string contentType)
    {
        // PDF has a dedicated entry; for other categories try the category key
        if (MaxSizes.TryGetValue(contentType, out var specific)) return specific;
        if (MaxSizes.TryGetValue(category.ToLowerInvariant(), out var general)) return general;

        // Fallback: 50 MB
        return 50L * 1024 * 1024;
    }

    /// <summary>
    /// Resolves the root storage path from configuration, defaulting to <c>wwwroot/uploads</c>.
    /// </summary>
    private string ResolveStoragePath()
    {
        var configured = _configuration.GetValue<string>("Media:StoragePath");
        if (!string.IsNullOrWhiteSpace(configured))
            return configured;

        // Fall back to a sensible default relative to the content root
        var contentRoot = _configuration.GetValue<string>("ContentRoot")
                          ?? _configuration.GetValue<string>("webRoot")
                          ?? Directory.GetCurrentDirectory();

        return Path.Combine(contentRoot, "wwwroot", "uploads");
    }

    /// <summary>
    /// Generates thumbnail files by copying the original (since SkiaSharp may not be available).
    /// The naming convention is <c>thumb{size}_{fileId}.ext</c>.
    /// When SkiaSharp is available, the image is actually resized.
    /// </summary>
    private async Task<(string? t150, string? t300, string? t600, string? t1200)> GenerateThumbnailsAsync(
        string originalPath,
        string targetDir,
        string fileId,
        string ext,
        string contentType,
        CancellationToken cancellationToken)
    {
        string? t150 = null, t300 = null, t600 = null, t1200 = null;

        foreach (var size in ThumbnailSizes)
        {
            var thumbFile = $"thumb{size}_{fileId}{ext}";
            var thumbPath = Path.Combine(targetDir, thumbFile);

            try
            {
                await ResizeImageAsync(originalPath, thumbPath, int.Parse(size), contentType, cancellationToken);
                // Track success for the result
                _ = size switch
                {
                    "150" => t150 = thumbFile,
                    "300" => t300 = thumbFile,
                    "600" => t600 = thumbFile,
                    "1200" => t1200 = thumbFile,
                    _ => (string?)null,
                };
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogWarning(ex, "Failed to generate thumbnail at {Size}px for {File}.", size, originalPath);
            }
        }

        return (t150, t300, t600, t1200);
    }

    /// <summary>
    /// Attempts to resize the source image to the specified width using SkiaSharp.
    /// Falls back to a plain file copy if SkiaSharp is unavailable or fails.
    /// </summary>
    private async Task ResizeImageAsync(
        string sourcePath,
        string destPath,
        int width,
        string contentType,
        CancellationToken cancellationToken)
    {
        // Try SkiaSharp first
        if (TryResizeWithSkiaSharp(sourcePath, destPath, width))
        {
            return;
        }

        // Fallback: copy original as-is (thumbnail won't be smaller, but file exists at the expected path)
        _logger.LogInformation(
            "SkiaSharp resize unavailable; copying original as thumbnail placeholder for {Path} at {Width}px.",
            destPath, width);

        await using var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        await using var dest = new FileStream(destPath, FileMode.Create, FileAccess.Write);
        await source.CopyToAsync(dest, cancellationToken);
    }

    /// <summary>
    /// Tries to resize an image using SkiaSharp. Returns <c>true</c> on success, <c>false</c> if SkiaSharp
    /// is not available or the operation fails.
    /// </summary>
    private static bool TryResizeWithSkiaSharp(string sourcePath, string destPath, int width)
    {
        try
        {
            // SkiaSharp types are loaded via the NuGet package reference.
            // If the package is missing at runtime this will throw a FileNotFoundException.
            using var inputStream = File.OpenRead(sourcePath);
            using var original = SkiaSharp.SKBitmap.Decode(inputStream);
            if (original is null) return false;

            var height = (int)((float)width / original.Width * original.Height);
            using var resized = original.Resize(new SkiaSharp.SKImageInfo(width, height), SkiaSharp.SKFilterQuality.Medium);
            if (resized is null) return false;

            using var image = SkiaSharp.SKImage.FromBitmap(resized);
            var extension = Path.GetExtension(destPath)?.ToLowerInvariant();

            SkiaSharp.SKEncodedImageFormat format = extension switch
            {
                ".jpg" or ".jpeg" => SkiaSharp.SKEncodedImageFormat.Jpeg,
                ".png" => SkiaSharp.SKEncodedImageFormat.Png,
                ".webp" => SkiaSharp.SKEncodedImageFormat.Webp,
                ".avif" => SkiaSharp.SKEncodedImageFormat.Avif,
                _ => SkiaSharp.SKEncodedImageFormat.Jpeg,
            };

            using var data = image.Encode(format, 85);
            using var outputStream = File.OpenWrite(destPath);
            data.SaveTo(outputStream);

            return true;
        }
        catch (FileNotFoundException)
        {
            // SkiaSharp assembly not deployed
            return false;
        }
        catch (DllNotFoundException)
        {
            // Native SkiaSharp binaries not available
            return false;
        }
        catch (TypeLoadException)
        {
            // SkiaSharp types not available
            return false;
        }
        catch (Exception ex)
        {
            // Any other SkiaSharp failure - log won't be available at this static scope
            System.Diagnostics.Debug.WriteLine($"SkiaSharp resize failed: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Attempts to read image dimensions from the file header without decoding the full bitmap.
    /// Returns (null, null) if the format is not recognised.
    /// </summary>
    private static (int? width, int? height) TryReadDimensions(string filePath)
    {
        try
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var reader = new BinaryReader(stream);

            // Read the first 24 bytes for header detection
            var header = reader.ReadBytes(24);
            if (header.Length < 24) return (null, null);

            // JPEG: starts with FF D8, dimensions at offset 0xA0 or after SOF marker
            if (header[0] == 0xFF && header[1] == 0xD8)
            {
                // Scan for Start Of Frame (SOF) marker 0xFF 0xC0/C1/C2
                long originalPosition = reader.BaseStream.Position;
                try
                {
                    reader.BaseStream.Seek(2, SeekOrigin.Begin);
                    while (reader.BaseStream.Position < reader.BaseStream.Length - 7)
                    {
                        var b = reader.ReadByte();
                        if (b == 0xFF)
                        {
                            var marker = reader.ReadByte();
                            if (marker >= 0xC0 && marker <= 0xC3)
                            {
                                // SOF0, SOF1, SOF2, SOF3
                                reader.ReadBytes(3); // skip length (2) + precision (1)
                                var h = (reader.ReadByte() << 8) | reader.ReadByte();
                                var w = (reader.ReadByte() << 8) | reader.ReadByte();
                                return (w, h);
                            }
                        }
                    }
                }
                finally
                {
                    reader.BaseStream.Seek(originalPosition, SeekOrigin.Begin);
                }
                return (null, null);
            }

            // PNG: starts with 89 50 4E 47 0D 0A 1A 0A, dimensions at offset 16
            if (header[0] == 0x89 && header[1] == 0x50 /*P*/ && header[2] == 0x4E /*N*/ && header[3] == 0x47 /*G*/)
            {
                var w = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
                var h = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
                return (w, h);
            }

            // WEBP: starts with 52 49 46 46 (RIFF), WEBP at offset 8, VP8/VP8L data follows
            if (header[0] == 0x52 /*R*/ && header[1] == 0x49 /*I*/ && header[2] == 0x46 /*F*/ && header[3] == 0x46 /*F*/)
            {
                // Lossy WEBP (VP8 ): dimensions at offset 26-29
                if (header[12] == 0x56 /*V*/ && header[13] == 0x50 /*P*/ && header[14] == 0x38 /*8*/ && header[15] == 0x20 /*space*/)
                {
                    // Need more bytes for VP8 header
                    reader.BaseStream.Seek(26, SeekOrigin.Begin);
                    var vp8Header = reader.ReadBytes(6);
                    if (vp8Header.Length >= 6)
                    {
                        var w = (vp8Header[0] | ((vp8Header[1] & 0x3F) << 8)) & 0x3FFF;
                        var h = ((vp8Header[2] & 0xFF) | ((vp8Header[3] & 0x3F) << 8)) & 0x3FFF;
                        return (w * 2, h * 2); // stored as (width/2, height/2) for VP8
                    }
                }
                // Lossless WEBP (VP8L): dimensions at offset 21-24
                if (header[12] == 0x56 /*V*/ && header[13] == 0x50 /*P*/ && header[14] == 0x38 /*8*/ && header[15] == 0x4C /*L*/)
                {
                    var bits = (header[21] << 0) | (header[22] << 8) | (header[23] << 16) | (header[24] << 24);
                    var w = (bits & 0x3FFF) + 1;
                    var h = ((bits >> 14) & 0x3FFF) + 1;
                    return (w, h);
                }
                return (null, null);
            }

            // AVIF / other formats – return null; actual dimensions can be read if SkiaSharp decodes the file
            return (null, null);
        }
        catch
        {
            // Best-effort; swallow failures
            return (null, null);
        }
    }
}
