namespace MusicEncyclopedia.Web.ViewModels;

/// <summary>
/// View model for the home page.
/// </summary>
public sealed class HomeViewModel
{
    public string CurrentCulture { get; set; } = "fa";
    public string MetaDescription { get; set; } = "";
    public string? MetaKeywords { get; set; }
    public string? CanonicalUrl { get; set; }

    // Featured content (will be populated from services later)
    public IReadOnlyList<FeaturedAlbumItem> FeaturedAlbums { get; set; } = [];
    public IReadOnlyList<FeaturedAlbumItem> LatestAlbums { get; set; } = [];
    public IReadOnlyList<FeaturedTrackItem> EssentialTracks { get; set; } = [];
    public IReadOnlyList<FeaturedPoemItem> FeaturedPoems { get; set; } = [];

    // Browse-by links
    public IReadOnlyList<BrowseLink> Genres { get; set; } = [];
    public IReadOnlyList<BrowseLink> Moods { get; set; } = [];
    public IReadOnlyList<BrowseLink> Instruments { get; set; } = [];
}

/// <summary>
/// Represents a featured album card on the home page.
/// </summary>
public sealed class FeaturedAlbumItem
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string? CoverUrl { get; set; }
    public string? Artist { get; set; }
    public int? Year { get; set; }
}

/// <summary>
/// Represents a featured track on the home page.
/// </summary>
public sealed class FeaturedTrackItem
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Artist { get; set; }
    public string? Duration { get; set; }
}

/// <summary>
/// Represents a featured poem on the home page.
/// </summary>
public sealed class FeaturedPoemItem
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Poet { get; set; }
}

/// <summary>
/// Represents a browse-by link (genre, mood, or instrument).
/// </summary>
public sealed class BrowseLink
{
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Controller { get; set; } = "";
}
