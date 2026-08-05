namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class ChartDetailViewModel
{
    public required ChartDetailDto Chart { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed record ChartDetailDto
{
    public int ChartId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Publisher { get; init; }
    public string? CountryName { get; init; }
    public string? Frequency { get; init; }

    // Chart entries
    public IReadOnlyList<ChartEntryDetailDto> Entries { get; init; } = [];
}

public sealed record ChartEntryDetailDto
{
    public int ChartEntryId { get; init; }
    public DateOnly Date { get; init; }
    public int Position { get; init; }
    public int? PreviousPosition { get; init; }
    public int? WeeksOnChart { get; init; }
    public int EntityTypeId { get; init; }
    public int EntityId { get; init; }
    public string? EntityTitle { get; init; }
    public string? EntitySlug { get; init; }
}
