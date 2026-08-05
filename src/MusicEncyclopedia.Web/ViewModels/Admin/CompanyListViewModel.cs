using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin company list page.
/// </summary>
public sealed class CompanyListViewModel
{
    public PagedResult<CompanyListItemDto> Items { get; init; } = PagedResult<CompanyListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a company in the admin list.
/// </summary>
public sealed class CompanyListItemDto
{
    public int CompanyId { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string? OriginalName { get; init; }
    public string? EnglishName { get; init; }
    public string? TypeName { get; init; }
    public string? CountryName { get; init; }
}
