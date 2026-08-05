namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the genre detail page (9.8).
/// </summary>
public sealed class GenreDetailViewModel
{
    /// <summary>Genre identifier.</summary>
    public int GenreId { get; set; }

    /// <summary>Genre display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Unique URL slug.</summary>
    public string Slug { get; set; } = "";

    /// <summary>Genre description / definition.</summary>
    public string? Description { get; set; }

    /// <summary>Parent genre, if any.</summary>
    public GenreParentDto? ParentGenre { get; set; }

    /// <summary>Child (sub) genres.</summary>
    public IReadOnlyList<GenreChildDto> ChildGenres { get; set; } = [];

    /// <summary>Albums classified under this genre.</summary>
    public IReadOnlyList<GenreAlbumDto> Albums { get; set; } = [];

    /// <summary>Tracks classified under this genre.</summary>
    public IReadOnlyList<GenreTrackDto> Tracks { get; set; } = [];

    /// <summary>Artists / people associated with albums or tracks in this genre.</summary>
    public IReadOnlyList<GenreArtistDto> Artists { get; set; } = [];

    /// <summary>Media items attached to this genre entity.</summary>
    public IReadOnlyList<GenreMediaDto> Media { get; set; } = [];

    /// <summary>Citations / references for the genre.</summary>
    public IReadOnlyList<GenreCitationDto> Citations { get; set; } = [];

    /// <summary>External links for the genre.</summary>
    public IReadOnlyList<GenreLinkDto> Links { get; set; } = [];

    /// <summary>The current culture for URL generation.</summary>
    public string Culture { get; set; } = "fa";
}

/// <summary>Parent genre reference.</summary>
public sealed class GenreParentDto
{
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}

/// <summary>Child / sub genre reference.</summary>
public sealed class GenreChildDto
{
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}

/// <summary>Album in this genre.</summary>
public sealed class GenreAlbumDto
{
    public int AlbumId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public DateOnly? ReleaseDate { get; init; }
    public string? CoverUrl { get; init; }
}

/// <summary>Track in this genre.</summary>
public sealed class GenreTrackDto
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public int? DurationSeconds { get; init; }
}

/// <summary>Artist / person associated with this genre.</summary>
public sealed class GenreArtistDto
{
    public int PersonId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}

/// <summary>Media attached to a genre entity.</summary>
public sealed class GenreMediaDto
{
    public int MediaId { get; init; }
    public string Url { get; init; } = "";
    public string? ThumbnailUrl { get; init; }
    public string? Description { get; init; }
    public string? MediaType { get; init; }
}

/// <summary>Citation / reference for a genre.</summary>
public sealed class GenreCitationDto
{
    public int CitationId { get; init; }
    public string? Quote { get; init; }
    public string? SourceName { get; init; }
    public string? SourceSlug { get; init; }
    public string? PageNumber { get; init; }
    public string? Url { get; init; }
}

/// <summary>External link for a genre.</summary>
public sealed class GenreLinkDto
{
    public string Url { get; init; } = "";
    public string? Title { get; init; }
    public string? LinkType { get; init; }
}
