using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the album detail page (9.3).
/// </summary>
public sealed class AlbumDetailViewModel
{
    /// <summary>
    /// The full album detail data.
    /// </summary>
    public required AlbumDetailDto Album { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
