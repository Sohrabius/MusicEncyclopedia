using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class AwardListViewModel
{
    public required PagedResult<AwardListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed class AwardListItemDto
{
    public int AwardId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Organization { get; init; }
    public string? CountryName { get; init; }
    public string? DescriptionPreview { get; set; }
}
