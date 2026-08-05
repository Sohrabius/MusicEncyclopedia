namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the instrument detail page (9.10).
/// </summary>
public sealed class InstrumentDetailViewModel
{
    /// <summary>Instrument identifier.</summary>
    public int InstrumentId { get; set; }

    /// <summary>Instrument display name.</summary>
    public string Name { get; set; } = "";

    /// <summary>Unique URL slug.</summary>
    public string Slug { get; set; } = "";

    /// <summary>Instrument description.</summary>
    public string? Description { get; set; }

    /// <summary>Instrument family name.</summary>
    public string? FamilyName { get; set; }

    /// <summary>Country of origin name.</summary>
    public string? CountryOfOrigin { get; set; }

    /// <summary>Historical notes about the instrument.</summary>
    public string? HistoricalNotes { get; set; }

    /// <summary>Musicians who play this instrument.</summary>
    public IReadOnlyList<InstrumentMusicianDto> Musicians { get; set; } = [];

    /// <summary>Tracks that feature this instrument.</summary>
    public IReadOnlyList<InstrumentTrackDto> Tracks { get; set; } = [];

    /// <summary>Track credits involving this instrument.</summary>
    public IReadOnlyList<InstrumentTrackCreditDto> TrackCredits { get; set; } = [];

    /// <summary>Media items attached to this instrument entity.</summary>
    public IReadOnlyList<InstrumentMediaDto> Media { get; set; } = [];

    /// <summary>Citations / references for the instrument.</summary>
    public IReadOnlyList<InstrumentCitationDto> Citations { get; set; } = [];

    /// <summary>The current culture for URL generation.</summary>
    public string Culture { get; set; } = "fa";
}

/// <summary>Musician who plays this instrument.</summary>
public sealed class InstrumentMusicianDto
{
    public int PersonId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Notes { get; init; }
}

/// <summary>Track that uses this instrument.</summary>
public sealed class InstrumentTrackDto
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public int? DurationSeconds { get; init; }
}

/// <summary>Track credit involving this instrument.</summary>
public sealed class InstrumentTrackCreditDto
{
    public int CreditId { get; init; }
    public string TrackTitle { get; init; } = "";
    public string TrackSlug { get; init; } = "";
    public string? PersonName { get; init; }
    public string? PersonSlug { get; init; }
}

/// <summary>Media attached to an instrument entity.</summary>
public sealed class InstrumentMediaDto
{
    public int MediaId { get; init; }
    public string Url { get; init; } = "";
    public string? ThumbnailUrl { get; init; }
    public string? Description { get; init; }
    public string? MediaType { get; init; }
}

/// <summary>Citation / reference for an instrument.</summary>
public sealed class InstrumentCitationDto
{
    public int CitationId { get; init; }
    public string? Quote { get; init; }
    public string? SourceName { get; init; }
    public string? SourceSlug { get; init; }
    public string? PageNumber { get; init; }
    public string? Url { get; init; }
}
