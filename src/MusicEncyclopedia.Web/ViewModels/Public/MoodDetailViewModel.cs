namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the mood detail page (9.9).
/// </summary>
public sealed class MoodDetailViewModel
{
    /// <summary>Mood identifier.</summary>
    public int MoodId { get; set; }

    /// <summary>Mood display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Unique URL slug.</summary>
    public string Slug { get; set; } = "";

    /// <summary>Mood description.</summary>
    public string? Description { get; set; }

    /// <summary>Albums tagged with this mood.</summary>
    public IReadOnlyList<MoodAlbumDto> Albums { get; set; } = [];

    /// <summary>Tracks tagged with this mood.</summary>
    public IReadOnlyList<MoodTrackDto> Tracks { get; set; } = [];

    /// <summary>Media items attached to this mood entity.</summary>
    public IReadOnlyList<MoodMediaDto> Media { get; set; } = [];

    /// <summary>Tags associated with this mood.</summary>
    public IReadOnlyList<MoodTagDto> Tags { get; set; } = [];

    /// <summary>The current culture for URL generation.</summary>
    public string Culture { get; set; } = "fa";
}

/// <summary>Album with this mood.</summary>
public sealed class MoodAlbumDto
{
    public int AlbumId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public DateOnly? ReleaseDate { get; init; }
    public string? CoverUrl { get; init; }
}

/// <summary>Track with this mood.</summary>
public sealed class MoodTrackDto
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public int? DurationSeconds { get; init; }
}

/// <summary>Media attached to a mood entity.</summary>
public sealed class MoodMediaDto
{
    public int MediaId { get; init; }
    public string Url { get; init; } = "";
    public string? ThumbnailUrl { get; init; }
    public string? Description { get; init; }
    public string? MediaType { get; init; }
}

/// <summary>Tag associated with a mood.</summary>
public sealed class MoodTagDto
{
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}
