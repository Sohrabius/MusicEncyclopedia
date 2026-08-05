using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin genre list page.
/// </summary>
public sealed class GenreListViewModel
{
    public PagedResult<GenreListItemDto> Items { get; init; } = PagedResult<GenreListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a genre in the admin list.
/// </summary>
public sealed class GenreListItemDto
{
    public int GenreId { get; init; }
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
    public string? ParentGenreName { get; init; }
}
