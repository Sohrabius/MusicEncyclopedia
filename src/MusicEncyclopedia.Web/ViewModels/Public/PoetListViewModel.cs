using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the poet listing page (spec 9.11).
/// A poet is a Person with PersonKind = "poet".
/// Reuses the same structure as PersonListViewModel but filtered.
/// </summary>
public sealed class PoetListViewModel
{
    /// <summary>
    /// Paginated list of poet items (NamedLinkDto: Slug + Name).
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
