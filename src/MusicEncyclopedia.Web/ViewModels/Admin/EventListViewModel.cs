using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class EventListViewModel
{
    public PagedResult<EventListItemDto> Items { get; init; } = PagedResult<EventListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class EventListItemDto
{
    public int PerformanceEventId { get; init; }
    public string Slug { get; init; } = "";
    public string? EventTypeName { get; init; }
    public DateTime? Date { get; init; }
    public string? VenueName { get; init; }
}
