namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a single search result item.
/// </summary>
public sealed class SearchResultDto
{
    public string EntityType { get; init; } = "";
    public int EntityId { get; init; }
    public string Title { get; init; } = "";
    public string? Subtitle { get; init; }
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string Url { get; init; } = "";
    public string Culture { get; init; } = "";
}
