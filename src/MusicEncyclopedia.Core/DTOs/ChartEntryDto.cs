namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a chart entry for an entity.
/// </summary>
public sealed class ChartEntryDto
{
    public int ChartEntryId { get; init; }
    public int ChartId { get; init; }
    public string ChartName { get; init; } = "";
    public string ChartSlug { get; init; } = "";
    public DateOnly Date { get; init; }
    public int Position { get; init; }
    public int? PreviousPosition { get; init; }
    public int? WeeksOnChart { get; init; }
}
