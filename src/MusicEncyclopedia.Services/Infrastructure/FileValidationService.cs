using Microsoft.AspNetCore.Http;

namespace MusicEncyclopedia.Services.Infrastructure;

/// <summary>
/// Validates uploaded files for size, MIME type, extension consistency, and path traversal safety.
/// Provides safe file name generation for storage.
/// </summary>
public sealed class FileValidationService
{
    /// <summary>
    /// Standard media type codes used throughout the application.
    /// </summary>
    public static class MediaTypeCodes
    {
        public const string Image = "IMAGE";
        public const string Audio = "AUDIO";
        public const string Video = "VIDEO";
        public const string Pdf = "PDF";
    }

    // MIME types allowed per media type code
    private static readonly Dictionary<string, HashSet<string>> AllowedMimeTypesByMediaType = new(StringComparer.OrdinalIgnoreCase)
    {
        [MediaTypeCodes.Image] = new(StringComparer.OrdinalIgnoreCase)
            { "image/jpeg", "image/png", "image/webp", "image/gif", "image/svg+xml" },
        [MediaTypeCodes.Audio] = new(StringComparer.OrdinalIgnoreCase)
            { "audio/mpeg", "audio/wav", "audio/flac", "audio/ogg", "audio/aac", "audio/mp4" },
        [MediaTypeCodes.Video] = new(StringComparer.OrdinalIgnoreCase)
            { "video/mp4", "video/webm", "video/ogg" },
        [MediaTypeCodes.Pdf] = new(StringComparer.OrdinalIgnoreCase)
            { "application/pdf" }
    };

    // Maps file extensions to their expected MIME types for consistency validation
    private static readonly Dictionary<string, HashSet<string>> ExtensionToMimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"]  = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg" },
        [".jpeg"] = new(StringComparer.OrdinalIgnoreCase) { "image/jpeg" },
        [".png"]  = new(StringComparer.OrdinalIgnoreCase) { "image/png" },
        [".webp"] = new(StringComparer.OrdinalIgnoreCase) { "image/webp" },
        [".gif"]  = new(StringComparer.OrdinalIgnoreCase) { "image/gif" },
        [".svg"]  = new(StringComparer.OrdinalIgnoreCase) { "image/svg+xml" },
        [".mp3"]  = new(StringComparer.OrdinalIgnoreCase) { "audio/mpeg" },
        [".wav"]  = new(StringComparer.OrdinalIgnoreCase) { "audio/wav" },
        [".flac"] = new(StringComparer.OrdinalIgnoreCase) { "audio/flac" },
        [".ogg"]  = new(StringComparer.OrdinalIgnoreCase) { "audio/ogg", "video/ogg" },
        [".aac"]  = new(StringComparer.OrdinalIgnoreCase) { "audio/aac" },
        [".mp4"]  = new(StringComparer.OrdinalIgnoreCase) { "audio/mp4", "video/mp4" },
        [".webm"] = new(StringComparer.OrdinalIgnoreCase) { "video/webm" },
        [".pdf"]  = new(StringComparer.OrdinalIgnoreCase) { "application/pdf" }
    };

    private const long DefaultMaxFileSize = 50 * 1024 * 1024; // 50 MB

    private readonly long _maxFileSize;

    /// <summary>
    /// Creates a new <see cref="FileValidationService"/> with the default maximum file size (50 MB).
    /// </summary>
    public FileValidationService() : this(DefaultMaxFileSize) { }

    /// <summary>
    /// Creates a new <see cref="FileValidationService"/> with a custom maximum file size.
    /// </summary>
    /// <param name="maxFileSize">Maximum allowed file size in bytes. Must be greater than zero.</param>
    public FileValidationService(long maxFileSize)
    {
        _maxFileSize = maxFileSize > 0 ? maxFileSize : DefaultMaxFileSize;
    }

    /// <summary>
    /// Validates the uploaded file against the specified media type code.
    /// </summary>
    /// <param name="file">The uploaded <see cref="IFormFile"/>.</param>
    /// <param name="mediaTypeCode">
    /// One of <see cref="MediaTypeCodes"/> values: IMAGE, AUDIO, VIDEO, or PDF.
    /// </param>
    /// <returns>A tuple indicating whether the file is valid and an optional error message.</returns>
    public (bool IsValid, string? ErrorMessage) Validate(IFormFile file, string mediaTypeCode)
    {
        // ── Null / empty check ──────────────────────────────────────────────────────
        if (file is null || file.Length == 0)
            return (false, "No file was provided or the file is empty.");

        // ── 1. File size check ──────────────────────────────────────────────────────
        if (file.Length > _maxFileSize)
            return (false, $"File size ({FormatSize(file.Length)}) exceeds the maximum allowed size of {FormatSize(_maxFileSize)}.");

        // ── 2. Path traversal check on the original file name ───────────────────────
        var fileName = file.FileName;
        if (string.IsNullOrWhiteSpace(fileName))
            return (false, "File name is required.");

        if (ContainsPathTraversal(fileName))
            return (false, "File name contains invalid characters (path traversal detected).");

        // ── 3. File extension validation ────────────────────────────────────────────
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension))
            return (false, "File is missing a file extension.");

        if (!ExtensionToMimeTypes.TryGetValue(extension, out var expectedMimeTypes))
            return (false, $"File extension \"{extension}\" is not supported. Allowed extensions: " +
                           string.Join(", ", ExtensionToMimeTypes.Keys.OrderBy(x => x)) + ".");

        // ── 4. MIME type vs. extension consistency check ────────────────────────────
        var contentType = file.ContentType;
        if (string.IsNullOrWhiteSpace(contentType))
        {
            var allowedExtensions = string.Join(", ", ExtensionToMimeTypes
                .Where(kvp => kvp.Value.Contains(extension, StringComparer.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key));
            return (false, $"Could not determine the MIME type of the uploaded file. Expected one of: {string.Join(", ", expectedMimeTypes)}.");
        }

        if (!expectedMimeTypes.Contains(contentType))
        {
            var expectedList = string.Join(", ", expectedMimeTypes);
            return (false, $"MIME type \"{contentType}\" does not match the file extension \"{extension}\". Expected one of: {expectedList}.");
        }

        // ── 5. Media-type-specific MIME type check ──────────────────────────────────
        if (!AllowedMimeTypesByMediaType.TryGetValue(mediaTypeCode, out var allowedForMediaType))
            return (false, $"Unknown media type code \"{mediaTypeCode}\". Must be one of: " +
                           string.Join(", ", AllowedMimeTypesByMediaType.Keys) + ".");

        if (!allowedForMediaType.Contains(contentType))
        {
            var allowedList = string.Join(", ", allowedForMediaType);
            return (false, $"MIME type \"{contentType}\" is not allowed for media type \"{mediaTypeCode}\". Allowed types: {allowedList}.");
        }

        return (true, null);
    }

    /// <summary>
    /// Infers the media type code (IMAGE, AUDIO, VIDEO, PDF) from a MIME type string.
    /// Returns <c>null</c> if the MIME type is not recognized.
    /// </summary>
    public static string? InferMediaTypeCode(string mimeType)
    {
        if (string.IsNullOrWhiteSpace(mimeType))
            return null;

        if (mimeType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return MediaTypeCodes.Image;

        if (mimeType.StartsWith("audio/", StringComparison.OrdinalIgnoreCase))
            return MediaTypeCodes.Audio;

        if (mimeType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
            return MediaTypeCodes.Video;

        if (mimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            return MediaTypeCodes.Pdf;

        return null;
    }

    /// <summary>
    /// Generates a safe, unique file name for storage.
    /// The original extension is preserved; the base name is replaced with a GUID
    /// to prevent path traversal and name collisions.
    /// </summary>
    /// <param name="originalFileName">The original file name from the upload.</param>
    /// <returns>A unique, sanitized file name safe for file-system storage (e.g., "a1b2c3d4e5f6.jpg").</returns>
    public static string GenerateSafeFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var safeName = $"{Guid.NewGuid():N}{extension}";
        return safeName;
    }

    /// <summary>
    /// The maximum allowed file size (in bytes) as configured for this service instance.
    /// </summary>
    public long MaxFileSize => _maxFileSize;

    /// <summary>
    /// Returns <c>true</c> if the file name contains path traversal characters
    /// (parent directory references, directory separators, or null characters).
    /// </summary>
    private static bool ContainsPathTraversal(string fileName)
    {
        return fileName.Contains("..", StringComparison.Ordinal) ||
               fileName.IndexOfAny(['/', '\\', '\0']) >= 0;
    }

    /// <summary>
    /// Formats a byte count into a human-readable string (B, KB, MB, GB).
    /// </summary>
    public static string FormatSize(long bytes)
    {
        return bytes switch
        {
            < 1024 => $"{bytes} B",
            < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
            < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024.0):F1} MB",
            _ => $"{bytes / (1024.0 * 1024.0 * 1024.0):F2} GB"
        };
    }
}
