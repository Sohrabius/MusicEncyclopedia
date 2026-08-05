using MusicEncyclopedia.Media.Models;

namespace MusicEncyclopedia.Media.Interfaces;

/// <summary>
/// Provides file upload, retrieval, and deletion operations for media assets.
/// </summary>
public interface IMediaService
{
    /// <summary>
    /// Uploads a file stream and persists metadata to the database.
    /// </summary>
    /// <param name="fileStream">The stream containing file data.</param>
    /// <param name="fileName">The original file name (including extension).</param>
    /// <param name="contentType">The MIME content type of the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A <see cref="MediaUploadResult"/> with metadata about the uploaded file.</returns>
    Task<MediaUploadResult> UploadAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    /// <summary>
    /// Soft-deletes a media record by its identifier.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteAsync(int mediaId, CancellationToken cancellationToken);

    /// <summary>
    /// Returns the public URL for a media item, optionally scaled to a thumbnail size.
    /// </summary>
    /// <param name="mediaId">The media identifier.</param>
    /// <param name="thumbnailSize">
    /// Optional thumbnail width in pixels. Valid values: 150, 300, 600, 1200.
    /// When null, the original-size URL is returned.
    /// </param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The absolute or relative URL of the media item.</returns>
    Task<string> GetUrlAsync(
        int mediaId,
        int? thumbnailSize = null,
        CancellationToken cancellationToken = default);
}
