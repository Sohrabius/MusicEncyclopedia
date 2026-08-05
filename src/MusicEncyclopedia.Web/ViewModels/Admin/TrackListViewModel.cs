using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin track list page (spec 10.6).
/// Wraps a paged result set together with the current search/filter state.
/// </summary>
public sealed class TrackListViewModel
{
    /// <summary>Paged list of track items for the current page.</summary>
    public PagedResult<TrackListItemDto> Items { get; init; } = PagedResult<TrackListItemDto>.Create([], 1, 20, 0);

    /// <summary>Current search query string.</summary>
    public string? SearchQuery { get; init; }

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; } = 20;
}
