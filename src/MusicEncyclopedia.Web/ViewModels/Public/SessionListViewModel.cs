using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the session listing page (9.14).
/// </summary>
public sealed class SessionListViewModel
{
    public required PagedResult<SessionListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
}

/// <summary>
/// Lightweight DTO for a session in the public list.
/// </summary>
public sealed class SessionListItemDto
{
    public int RecordingSessionId { get; init; }
    public string Slug { get; init; } = "";
    public string? SessionTypeName { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? LocationName { get; init; }
    public string? NotesPreview { get; set; }
}
