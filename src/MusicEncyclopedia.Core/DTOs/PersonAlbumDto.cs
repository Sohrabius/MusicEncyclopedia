namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// An album in a person's discography (spec 9.6 §4), carrying the aggregated
/// roles and instruments the person performed under so the page can filter
/// by category, year, role and instrument.
/// </summary>
public sealed class PersonAlbumDto
{
    public int AlbumId { get; init; }

    /// <summary>
    /// The album's Entity row id — used to overlay localized titles (spec 7.3).
    /// </summary>
    public int EntityId { get; init; }

    public string Slug { get; init; } = "";
    public string Title { get; set; } = "";
    public string? OriginalTitle { get; set; }
    public string? EnglishTitle { get; set; }
    public string? CategoryName { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }

    /// <summary>Distinct role names the person holds on this album.</summary>
    public IReadOnlyList<string> Roles { get; set; } = [];

    /// <summary>Distinct instruments the person plays on this album.</summary>
    public IReadOnlyList<string> Instruments { get; set; } = [];
}
