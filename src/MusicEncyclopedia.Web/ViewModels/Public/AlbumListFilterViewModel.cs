namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// Represents the current filter state for the album listing page.
/// </summary>
public sealed class AlbumListFilterViewModel
{
    public string? GenreSlug { get; init; }
    public string? MoodSlug { get; init; }
    public int? Year { get; init; }
    public string? SearchQuery { get; init; }
    public string? Sort { get; init; }
    public int Page { get; init; } = 1;
}
