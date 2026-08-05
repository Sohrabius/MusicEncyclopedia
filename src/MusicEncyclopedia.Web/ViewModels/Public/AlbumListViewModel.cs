using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the album listing page (9.2).
/// </summary>
public sealed class AlbumListViewModel
{
    /// <summary>
    /// Paginated list of album items.
    /// </summary>
    public required PagedResult<AlbumListItemDto> Items { get; init; }

    /// <summary>
    /// Current filter state (used to preserve selections across pagination).
    /// </summary>
    public AlbumListFilterViewModel CurrentFilters { get; init; } = new();

    /// <summary>
    /// Available genres for the filter dropdown.
    /// </summary>
    public IReadOnlyList<NamedLinkDto> Genres { get; init; } = [];

    /// <summary>
    /// Available moods for the filter dropdown.
    /// </summary>
    public IReadOnlyList<NamedLinkDto> Moods { get; init; } = [];

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
