using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin album list page (spec 10.4).
/// Wraps a paged result set together with the current search/filter state.
/// </summary>
public sealed class AlbumListViewModel
{
    /// <summary>Paged list of album items for the current page.</summary>
    public PagedResult<AlbumListItemDto> Items { get; init; } = PagedResult<AlbumListItemDto>.Create([], 1, 20, 0);

    /// <summary>Current search query string.</summary>
    public string? SearchQuery { get; init; }

    /// <summary>Current page number (1-based).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Number of items per page.</summary>
    public int PageSize { get; init; } = 20;
}
