using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides query operations for tracks.
/// </summary>
public interface ITrackQueryService
{
    /// <summary>
    /// Gets a paginated list of tracks for the specified culture.
    /// </summary>
    Task<PagedResult<TrackDetailDto>> GetTracksAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? genre = null,
        string? mood = null,
        string? artist = null,
        string? q = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the full detail of a track by its slug.
    /// </summary>
    Task<TrackDetailDto?> GetTrackBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default);
}
