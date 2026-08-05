namespace MusicEncyclopedia.Media.Models;

/// <summary>
/// Result returned after a successful media upload, containing metadata and URLs.
/// </summary>
public sealed class MediaUploadResult
{
    /// <summary>Database-generated media identifier.</summary>
    public int MediaId { get; init; }

    /// <summary>The original file name as uploaded.</summary>
    public string FileName { get; init; } = "";

    /// <summary>Public URL for the original (full-size) media file.</summary>
    public string Url { get; init; } = "";

    /// <summary>URL for the 150px-wide thumbnail (images only).</summary>
    public string? ThumbnailUrl150 { get; init; }

    /// <summary>URL for the 300px-wide thumbnail (images only).</summary>
    public string? ThumbnailUrl300 { get; init; }

    /// <summary>URL for the 600px-wide thumbnail (images only).</summary>
    public string? ThumbnailUrl600 { get; init; }

    /// <summary>URL for the 1200px-wide thumbnail (images only).</summary>
    public string? ThumbnailUrl1200 { get; init; }

    /// <summary>Size of the uploaded file in bytes.</summary>
    public long FileSize { get; init; }

    /// <summary>MIME type of the uploaded file.</summary>
    public string MimeType { get; init; } = "";

    /// <summary>Width of the media in pixels (null for non-image files).</summary>
    public int? Width { get; init; }

    /// <summary>Height of the media in pixels (null for non-image files).</summary>
    public int? Height { get; init; }
}
