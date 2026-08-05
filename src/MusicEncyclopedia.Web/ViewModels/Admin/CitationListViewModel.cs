using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin citation list page.
/// </summary>
public sealed class CitationListViewModel
{
    public PagedResult<CitationListItemDto> Items { get; init; } = PagedResult<CitationListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a citation in the admin list.
/// </summary>
public sealed class CitationListItemDto
{
    public int CitationId { get; init; }
    public string? EntityTypeName { get; init; }
    public int EntityId { get; init; }
    public string? SourceTitle { get; init; }
    public string? QuotePreview { get; init; }
    public string? FieldName { get; init; }
    public string? PageNumber { get; init; }
}
