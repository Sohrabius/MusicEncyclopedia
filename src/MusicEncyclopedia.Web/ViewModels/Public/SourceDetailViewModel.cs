namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class SourceDetailViewModel
{
    public required SourceDetailDto Source { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed record SourceDetailDto
{
    public int SourceId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? SourceTypeName { get; init; }
    public string? Author { get; init; }
    public string? PublisherName { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public string? Url { get; init; }

    // Citations (facts referencing this source)
    public IReadOnlyList<SourceCitationDto> Citations { get; init; } = [];
}

public sealed record SourceCitationDto
{
    public int CitationId { get; init; }
    public int EntityTypeId { get; init; }
    public int EntityId { get; init; }
    public string? FieldName { get; init; }
    public string? Quote { get; init; }
    public string? PageNumber { get; init; }
    public string? EntityTitle { get; init; }
}
