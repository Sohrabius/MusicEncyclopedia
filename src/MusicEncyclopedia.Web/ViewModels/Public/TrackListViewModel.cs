using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the track listing page (9.4).
/// </summary>
public sealed class TrackListViewModel
{
    /// <summary>
    /// Paginated list of track items.
    /// </summary>
    public required PagedResult<TrackDetailDto> Items { get; init; }

    /// <summary>
    /// Current filter state.
    /// </summary>
    public TrackListFilterViewModel CurrentFilters { get; init; } = new();

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
