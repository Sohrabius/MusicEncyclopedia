namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an award assignment to an entity.
/// </summary>
public sealed class AwardAssignmentDto
{
    public int AwardAssignmentId { get; init; }
    public int AwardId { get; init; }
    public string AwardName { get; init; } = "";
    public string AwardSlug { get; init; } = "";
    public DateOnly? AwardDate { get; init; }
    public string? Result { get; init; }
    public string? Category { get; init; }
}
