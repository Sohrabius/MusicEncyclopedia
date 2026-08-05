namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a search query input.
/// </summary>
public sealed class SearchQuery
{
    public string? Q { get; init; }
    public string? EntityType { get; init; }
    public string? Genre { get; init; }
    public string? Mood { get; init; }
    public string? Instrument { get; init; }
    public string? Language { get; init; }
    public string? Culture { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
