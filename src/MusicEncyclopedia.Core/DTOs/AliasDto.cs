namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an alias (alternative name) for an entity.
/// </summary>
public sealed class AliasDto
{
    public int AliasId { get; init; }
    public string AliasName { get; init; } = "";
    public string? AliasType { get; init; }
    public string? Language { get; init; }
    public bool IsPrimary { get; init; }
    public string? Notes { get; init; }
}
