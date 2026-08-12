namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// A performance event associated with an album or track (spec 4.3).
/// </summary>
public sealed class PerformanceEventDto
{
    public int PerformanceEventId { get; init; }
    public string Slug { get; init; } = "";
    public DateTime? Date { get; init; }
    public string? EventTypeName { get; init; }
    public string? LocationName { get; init; }
    public string? PerformanceNotes { get; init; }
}
