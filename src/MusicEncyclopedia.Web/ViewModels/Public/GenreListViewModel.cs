using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the genre listing page (9.8).
/// </summary>
public sealed class GenreListViewModel
{
    /// <summary>
    /// Paginated list of genre items.
    /// </summary>
    public required PagedResult<NamedLinkDto> Items { get; init; }

    /// <summary>
    /// The current search query string, if any.
    /// </summary>
    public string? SearchQuery { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
