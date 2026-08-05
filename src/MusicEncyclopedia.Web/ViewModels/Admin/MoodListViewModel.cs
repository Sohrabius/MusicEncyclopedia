using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin mood list page.
/// </summary>
public sealed class MoodListViewModel
{
    public PagedResult<MoodListItemDto> Items { get; init; } = PagedResult<MoodListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a mood in the admin list.
/// </summary>
public sealed class MoodListItemDto
{
    public int MoodId { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
}
