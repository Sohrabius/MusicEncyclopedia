using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin media list page.
/// </summary>
public sealed class MediaListViewModel
{
    public PagedResult<MediaListItemDto> Items { get; init; } = PagedResult<MediaListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public string? MediaTypeFilter { get; init; }
    public int? AssignedEntityTypeId { get; init; }
    public int? AssignedEntityId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public IReadOnlyList<Data.Entities.MediaType> MediaTypes { get; init; } = [];
    public IReadOnlyList<Data.Entities.EntityType> EntityTypes { get; init; } = [];
}

/// <summary>
/// Lightweight DTO for displaying a media item in the admin list.
/// </summary>
public sealed class MediaListItemDto
{
    public int MediaId { get; init; }
    public string FileName { get; init; } = "";
    public string? MediaTypeName { get; init; }
    public string? MediaTypeCode { get; init; }
    public long FileSize { get; init; }
    public string? FormattedFileSize { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public string? ThumbnailUrl150 { get; init; }
    public int AssignmentCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
