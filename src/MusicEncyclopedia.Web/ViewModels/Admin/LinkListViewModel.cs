using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin entity links list page.
/// </summary>
public sealed class LinkListViewModel
{
    public PagedResult<LinkListItemDto> Items { get; init; } = PagedResult<LinkListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int? EntityTypeIdFilter { get; init; }
    public int? EntityIdFilter { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public IReadOnlyList<Data.Entities.EntityType> EntityTypes { get; init; } = [];
}

/// <summary>
/// Lightweight DTO for displaying an entity link in the admin list.
/// </summary>
public sealed class LinkListItemDto
{
    public int EntityLinkId { get; init; }
    public string? EntityTypeName { get; init; }
    public int EntityId { get; init; }
    public string? LinkTypeName { get; init; }
    public string Url { get; init; } = "";
    public string? Title { get; init; }
}
