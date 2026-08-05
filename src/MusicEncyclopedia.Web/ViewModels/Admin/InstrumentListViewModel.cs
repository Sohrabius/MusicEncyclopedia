using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin instrument list page.
/// </summary>
public sealed class InstrumentListViewModel
{
    public PagedResult<InstrumentListItemDto> Items { get; init; } = PagedResult<InstrumentListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying an instrument in the admin list.
/// </summary>
public sealed class InstrumentListItemDto
{
    public int InstrumentId { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string? FamilyName { get; init; }
    public string? CountryName { get; init; }
}
