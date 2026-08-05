using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the person listing page.
/// Spec 9.6 — list of people with search support.
/// </summary>
public sealed class PersonListViewModel
{
    /// <summary>
    /// Paginated list of person items.
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
