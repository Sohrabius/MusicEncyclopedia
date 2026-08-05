namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a tag assigned to an entity.
/// </summary>
public sealed class TagDto
{
    public int TagId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
}
