using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin poem list page (spec 10.9).
/// </summary>
public sealed class PoemListViewModel
{
    public PagedResult<PoemListItemDto> Items { get; init; } = PagedResult<PoemListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a poem in the admin list.
/// </summary>
public sealed class PoemListItemDto
{
    public int PoemId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public string? PoetName { get; init; }
    public DateOnly? OriginalPublicationDate { get; init; }
}
