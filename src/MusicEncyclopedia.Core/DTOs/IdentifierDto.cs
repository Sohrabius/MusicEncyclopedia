namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an identifier (e.g., barcode, catalog number) for an entity.
/// </summary>
public sealed class IdentifierDto
{
    public int IdentifierId { get; init; }
    public string IdentifierType { get; init; } = "";
    public string IdentifierValue { get; init; } = "";
}
