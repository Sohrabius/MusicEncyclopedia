using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class LocationListViewModel
{
    public required PagedResult<LocationListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
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
