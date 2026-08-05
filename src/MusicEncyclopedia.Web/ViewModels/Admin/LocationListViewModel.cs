using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class LocationListViewModel
{
    public PagedResult<LocationListItemDto> Items { get; init; } = PagedResult<LocationListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class LocationListItemDto
{
    public int LocationId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? LocationTypeName { get; init; }
    public string? ParentLocationName { get; init; }
    public string? CountryName { get; init; }
}
