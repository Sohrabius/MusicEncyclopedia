using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class PublicationListViewModel
{
    public required PagedResult<PublicationListItemDto> Items { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed class PublicationListItemDto
{
    public int PublicationId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? PublicationTypeName { get; init; }
    public string? PublisherName { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public string? ISBN { get; init; }
}

public sealed class PublicationDetailViewModel
{
    public required PublicationDetailDto Publication { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed record PublicationDetailDto
{
    public int PublicationId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? PublicationTypeName { get; init; }
    public string? PublisherName { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public string? ISBN { get; init; }
    public string? AuthorName { get; init; }
    public string? AuthorSlug { get; init; }
    public List<PublicationPoemDto> Poems { get; init; } = new();
}

public sealed class PublicationPoemDto
{
    public int PoemId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
}
