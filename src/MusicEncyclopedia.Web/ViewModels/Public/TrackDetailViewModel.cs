using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the track detail page (9.5).
/// </summary>
public sealed class TrackDetailViewModel
{
    /// <summary>
    /// The full track detail data.
    /// </summary>
    public required TrackDetailDto Track { get; init; }

    /// <summary>
    /// Whether the lyrics section should be visible based on availability rules.
    /// </summary>
    public bool LyricsSectionVisible { get; init; }

    /// <summary>
    /// A message to display when lyrics are not available (e.g., instrumental notice, restricted message).
    /// </summary>
    public string? LyricsMessage { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
