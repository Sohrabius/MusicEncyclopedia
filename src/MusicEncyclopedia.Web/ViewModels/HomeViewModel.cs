using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels;

/// <summary>
/// View model for the album-first home page (HOME_REDESIGN_PLAN.md).
/// </summary>
public sealed class HomeViewModel
{
    public string CurrentCulture { get; set; } = "fa";
    public string MetaDescription { get; set; } = "";
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }

    // Banner hint chips (popular genres / moods / instruments)
    public IReadOnlyList<BrowseLink> Genres { get; set; } = [];
    public IReadOnlyList<BrowseLink> Moods { get; set; } = [];
    public IReadOnlyList<BrowseLink> Instruments { get; set; } = [];

    // Album catalog — category filter chips + the first page of albums
    public IReadOnlyList<AlbumCategoryChip> Categories { get; set; } = [];
    public string? SelectedCategory { get; set; }
    public PagedResult<AlbumListItemDto>? Albums { get; set; }

    /// <summary>
    /// Total albums visible after the currently rendered page (used by the
    /// "Load more" counter and the lazy-load → pagination switch).
    /// </summary>
    public int LoadedCount { get; set; }

    // Sidebar — random poem (per request, never cached) + About card
    public RandomPoemCard? RandomPoem { get; set; }
    public AboutStats About { get; set; } = new();
}

/// <summary>
/// A single album-category filter chip on the home page.
/// </summary>
public sealed class AlbumCategoryChip
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public int AlbumCount { get; set; }
}

/// <summary>
/// A random poem displayed in the home sidebar, with the track (and its album)
/// that performs the poem — resolved per page load, never cached.
/// </summary>
public sealed class RandomPoemCard
{
    public string PoemSlug { get; set; } = "";
    public string PoemTitle { get; set; } = "";
    public string? Poet { get; set; }
    public string? TrackSlug { get; set; }
    public string? TrackTitle { get; set; }
    public string? AlbumSlug { get; set; }
    public string? AlbumTitle { get; set; }
}

/// <summary>
/// Live catalog counts shown in the About card.
/// </summary>
public sealed class AboutStats
{
    public int Albums { get; set; }
    public int Tracks { get; set; }
    public int People { get; set; }
}

/// <summary>
/// Represents a browse-by link (genre, mood, or instrument) on the home banner.
/// </summary>
public sealed class BrowseLink
{
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
}

/// <summary>
/// Cached banner hint chips (genres / moods / instruments) for the home page.
/// </summary>
public sealed class HomeBrowseData
{
    public IReadOnlyList<BrowseLink> Genres { get; set; } = [];
    public IReadOnlyList<BrowseLink> Moods { get; set; } = [];
    public IReadOnlyList<BrowseLink> Instruments { get; set; } = [];
}
