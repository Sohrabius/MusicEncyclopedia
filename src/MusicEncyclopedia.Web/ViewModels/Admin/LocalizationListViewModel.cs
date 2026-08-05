using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin localization list page.
/// </summary>
public sealed class LocalizationListViewModel
{
    public PagedResult<LocalizationListItemDto> Items { get; init; } = PagedResult<LocalizationListItemDto>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Lightweight DTO for displaying a localization in the admin list.
/// </summary>
public sealed class LocalizationListItemDto
{
    public int LocalizationId { get; init; }
    public string? EntityTypeName { get; init; }
    public int EntityId { get; init; }
    public string? LanguageName { get; init; }
    public string? LanguageCode { get; init; }
    public string FieldName { get; init; } = "";
    public string LocalizedTextPreview { get; init; } = "";
}
