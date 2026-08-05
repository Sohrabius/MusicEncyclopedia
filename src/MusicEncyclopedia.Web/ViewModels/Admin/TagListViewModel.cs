using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin tag list page.
/// </summary>
public sealed class TagListViewModel
{
    public PagedResult<TagListItemDto> Items { get; init; } = PagedResult<TagListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a tag in the admin list.
/// </summary>
public sealed class TagListItemDto
{
    public int TagId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? DescriptionPreview { get; init; }
    public int AssignmentCount { get; init; }
}
