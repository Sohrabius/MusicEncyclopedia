using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin sung version list page (spec 10.10).
/// </summary>
public sealed class SungVersionListViewModel
{
    public PagedResult<SungVersionListItemDto> Items { get; init; } = PagedResult<SungVersionListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a sung version in the admin list.
/// </summary>
public sealed class SungVersionListItemDto
{
    public int SungVersionId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? PoemTitle { get; init; }
    public string? VocalStyleName { get; init; }
    public bool IsCanonical { get; init; }
}
