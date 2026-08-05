using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class ChartListViewModel
{
    public PagedResult<ChartListItemDto> Items { get; init; } = PagedResult<ChartListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
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
