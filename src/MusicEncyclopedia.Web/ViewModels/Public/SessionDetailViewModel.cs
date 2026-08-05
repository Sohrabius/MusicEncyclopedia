namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the session detail page (9.14).
/// </summary>
public sealed class SessionDetailViewModel
{
    public required SessionDetailDto Session { get; init; }
    public string Culture { get; init; } = "fa";
}

/// <summary>
/// Full session detail data.
/// </summary>
public sealed record SessionDetailDto
{
    public int RecordingSessionId { get; init; }
    public string Slug { get; init; } = "";
    public string? SessionTypeName { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? DatePrecision { get; init; }
    public string? Notes { get; init; }

    // Location
    public int? LocationId { get; init; }
    public string? LocationName { get; init; }
    public string? LocationSlug { get; init; }

    // Albums recorded
    public IReadOnlyList<SessionAlbumDto> Albums { get; init; } = [];

    // Tracks recorded
    public IReadOnlyList<SessionTrackDto> Tracks { get; init; } = [];

    // Credits (musicians)
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];

    // Media
    public IReadOnlyList<MediaDto> Media { get; init; } = [];

    // Citations
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
}

public sealed record SessionAlbumDto
{
    public int AlbumId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? CoverUrl { get; init; }
}

public sealed record SessionTrackDto
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public int? DurationSeconds { get; init; }
}
