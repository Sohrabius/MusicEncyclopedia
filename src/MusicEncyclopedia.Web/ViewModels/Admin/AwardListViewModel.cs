using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class AwardListViewModel
{
    public PagedResult<AwardListItemDto> Items { get; init; } = PagedResult<AwardListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class AwardListItemDto
{
    public int AwardId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Organization { get; init; }
    public string? CountryName { get; init; }
}
