namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// A single event on a person's career timeline (spec 9.6 §3).
/// Aggregates album/track releases, recording sessions, performance
/// events, awards, chart entries and publications into one date-sorted feed.
/// </summary>
public sealed class TimelineEntryDto
{
    /// <summary>
    /// Machine-readable kind: AlbumRelease, TrackRelease, RecordingSession,
    /// PerformanceEvent, Award, ChartEntry or Publication.
    /// </summary>
    public string Type { get; init; } = "";

    public string Title { get; init; } = "";

    /// <summary>
    /// Exact date when known (album releases, sessions, events, chart entries, publications).
    /// </summary>
    public DateTime? Date { get; init; }

    /// <summary>
    /// Year-only events (awards) use this instead of <see cref="Date"/>.
    /// </summary>
    public int? Year { get; init; }

    /// <summary>
    /// Slug of the linked entity — null when the entity has no public route (publications).
    /// </summary>
    public string? Slug { get; init; }

    /// <summary>
    /// Public route segment used to build the link: albums, tracks, sessions,
    /// events, awards, charts, poems. Null when no public route exists.
    /// </summary>
    public string? RouteName { get; init; }

    /// <summary>
    /// Optional secondary text (venue, chart position, award result, location).
    /// </summary>
    public string? Detail { get; init; }
}
