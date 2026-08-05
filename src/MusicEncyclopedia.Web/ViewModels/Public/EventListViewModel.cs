using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class EventListViewModel
{
    public required PagedResult<EventListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed class EventListItemDto
{
    public int PerformanceEventId { get; init; }
    public string Slug { get; init; } = "";
    public string? EventTypeName { get; init; }
    public DateTime? Date { get; init; }
    public string? VenueName { get; init; }
    public string? PerformanceNotesPreview { get; set; }
}
