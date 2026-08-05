namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a certification assignment to an entity.
/// </summary>
public sealed class CertificationAssignmentDto
{
    public int CertificationAssignmentId { get; init; }
    public int CertificationId { get; init; }
    public string CertificationName { get; init; } = "";
    public string? CertificationSlug { get; init; }
    public DateOnly? CertificationDate { get; init; }
    public string? Country { get; init; }
}
