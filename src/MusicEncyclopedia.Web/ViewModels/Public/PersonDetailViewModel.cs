using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the person detail page (9.6).
/// Carries the typed detail DTO plus the active discography / contribution filters.
/// </summary>
public sealed class PersonDetailViewModel
{
    public required PersonDetailDto Person { get; init; }

    /// <summary>The current culture for URL generation.</summary>
    public string Culture { get; init; } = "fa";

    // ── Active discography filters (4.2) ──
    public string? AlbumCategory { get; init; }
    public int? AlbumYear { get; init; }
    public string? AlbumRole { get; init; }
    public string? AlbumInstrument { get; init; }

    // ── Active track contribution filters (4.2) ──
    public string? ContributionRole { get; init; }
    public string? ContributionInstrument { get; init; }
    public string? ContributionAlbum { get; init; }
    public string? ContributionGenre { get; init; }
}
