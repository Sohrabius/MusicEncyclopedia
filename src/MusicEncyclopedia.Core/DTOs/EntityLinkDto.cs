namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an external link attached to an entity.
/// </summary>
public sealed class EntityLinkDto
{
    public int EntityLinkId { get; init; }
    public string Url { get; init; } = "";
    public string? LinkType { get; init; }
    public string? Title { get; init; }
}
