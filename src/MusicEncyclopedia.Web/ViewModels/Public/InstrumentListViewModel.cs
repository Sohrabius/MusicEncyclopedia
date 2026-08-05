using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the instrument listing page (9.10).
/// </summary>
public sealed class InstrumentListViewModel
{
    /// <summary>
    /// Paginated list of instrument items.
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
