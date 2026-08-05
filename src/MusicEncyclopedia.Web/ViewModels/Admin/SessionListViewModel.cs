using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class SessionListViewModel
{
    public PagedResult<SessionListItemDto> Items { get; init; } = PagedResult<SessionListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

public sealed class SessionListItemDto
{
    public int RecordingSessionId { get; init; }
    public string Slug { get; init; } = "";
    public string? SessionTypeName { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? LocationName { get; init; }
}
