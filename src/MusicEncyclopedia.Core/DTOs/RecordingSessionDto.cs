namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// A recording session associated with an album or track (spec 4.3).
/// </summary>
public sealed class RecordingSessionDto
{
    public int RecordingSessionId { get; init; }
    public string Slug { get; init; } = "";
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? SessionTypeName { get; init; }
    public string? LocationName { get; init; }
    public string? Notes { get; init; }
}
