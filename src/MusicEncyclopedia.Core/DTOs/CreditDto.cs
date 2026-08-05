namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a single credit (person or company with a role) for an entity.
/// </summary>
public sealed class CreditDto
{
    public int CreditId { get; init; }
    public int? PersonId { get; init; }
    public string? PersonFullName { get; init; }
    public string? PersonSlug { get; init; }
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public string? CompanySlug { get; init; }
    public string RoleName { get; init; } = "";
    public string? RoleCode { get; init; }
    public int RoleDisplayOrder { get; init; }
    public string? InstrumentName { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsPrimary { get; init; }
    public string? Notes { get; init; }
}
