using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class ChartListViewModel
{
    public required PagedResult<ChartListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed class ChartListItemDto
{
    public int ChartId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Publisher { get; init; }
    public string? CountryName { get; init; }
    public string? Frequency { get; init; }
}
