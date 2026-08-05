using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the company listing page.
/// Spec 9.7 — list of companies with search support.
/// </summary>
public sealed class CompanyListViewModel
{
    /// <summary>
    /// Paginated list of company items.
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
