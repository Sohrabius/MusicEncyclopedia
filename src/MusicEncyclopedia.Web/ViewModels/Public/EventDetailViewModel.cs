namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class EventDetailViewModel
{
    public required EventDetailDto Event { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed record EventDetailDto
{
    public int PerformanceEventId { get; init; }
    public string Slug { get; init; } = "";
    public string? EventTypeName { get; init; }
    public DateTime? Date { get; init; }
    public string? AudienceInfo { get; init; }
    public string? PerformanceNotes { get; init; }
    public string? ImprovisationNotes { get; init; }

    // Location / Venue
    public int? LocationId { get; init; }
    public string? VenueName { get; init; }
    public string? VenueSlug { get; init; }

    // Albums performed
    public IReadOnlyList<EventAlbumDto> Albums { get; init; } = [];

    // Tracks performed
    public IReadOnlyList<EventTrackDto> Tracks { get; init; } = [];

    // Credits (attendees / performers)
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];

    // Media
    public IReadOnlyList<MediaDto> Media { get; init; } = [];

    // Citations
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
}

public sealed record EventAlbumDto
{
    public int AlbumId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? CoverUrl { get; init; }
}

public sealed record EventTrackDto
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public int? DurationSeconds { get; init; }
}
