using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides query operations for albums.
/// </summary>
public interface IAlbumQueryService
{
    /// <summary>
    /// Gets a paginated list of albums for the specified culture.
    /// </summary>
    Task<PagedResult<AlbumListItemDto>> GetAlbumsAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? category = null,
        string? genre = null,
        string? mood = null,
        string? q = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full detail of an album by its slug.
    /// </summary>
    Task<AlbumDetailDto?> GetAlbumBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the track list for a specific album.
    /// </summary>
    Task<IReadOnlyList<AlbumTrackDto>> GetAlbumTracksAsync(
        int albumId,
        CancellationToken cancellationToken = default);
}
