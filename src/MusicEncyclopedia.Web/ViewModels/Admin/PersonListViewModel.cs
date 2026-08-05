using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin person list page (spec 10.8).
/// </summary>
public sealed class PersonListViewModel
{
    public PagedResult<PersonListItemDto> Items { get; init; } = PagedResult<PersonListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a person in the admin list.
/// </summary>
public sealed class PersonListItemDto
{
    public int PersonId { get; init; }
    public string Slug { get; init; } = "";
    public string FullName { get; init; } = "";
    public string? OriginalName { get; init; }
    public string? EnglishName { get; init; }
    public string? KindName { get; init; }
    public DateOnly? BirthDate { get; init; }
    public DateOnly? DeathDate { get; init; }
}
