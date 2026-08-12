namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// A track a person contributed to (spec 9.6 §5), shown per album appearance
/// with aggregated roles, instruments and genres so the page can filter by
/// role, instrument, album and genre.
/// </summary>
public sealed class TrackContributionDto
{
    public int TrackId { get; init; }

    /// <summary>
    /// The track's Entity row id — used to overlay localized titles (spec 7.3).
    /// </summary>
    public int EntityId { get; init; }

    public string Slug { get; init; } = "";
    public string Title { get; set; } = "";
    public string? OriginalTitle { get; set; }
    public string? EnglishTitle { get; set; }

    public int? AlbumId { get; init; }
    public string? AlbumTitle { get; init; }
    public string? AlbumSlug { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public string? CoverUrl { get; init; }

    /// <summary>Distinct role names the person holds on this track.</summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>Distinct instruments the person plays on this track.</summary>
    public IReadOnlyList<string> Instruments { get; set; } = [];

    /// <summary>Distinct genre names of this track.</summary>
    public IReadOnlyList<string> Genres { get; set; } = [];
}
