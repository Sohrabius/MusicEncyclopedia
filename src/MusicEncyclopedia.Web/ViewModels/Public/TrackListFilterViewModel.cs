namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// Represents the current filter state for the track listing page.
/// </summary>
public sealed class TrackListFilterViewModel
{
    public string? GenreSlug { get; init; }
    public string? ArtistSlug { get; init; }
    public string? SearchQuery { get; init; }
    public bool? IsInstrumental { get; init; }
    public int Page { get; init; } = 1;
}
