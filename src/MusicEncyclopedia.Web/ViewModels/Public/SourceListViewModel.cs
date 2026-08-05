using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class SourceListViewModel
{
    public required PagedResult<SourceListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed class SourceListItemDto
{
    public int SourceId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? SourceTypeName { get; init; }
    public string? Author { get; init; }
    public string? PublisherName { get; init; }
    public DateOnly? PublicationDate { get; init; }
}
