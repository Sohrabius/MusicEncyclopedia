namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a citation attached to an entity or field.
/// </summary>
public sealed class CitationDto
{
    public int CitationId { get; init; }
    public int? SourceId { get; init; }
    public string? SourceName { get; init; }
    public string? SourceSlug { get; init; }
    public string? FieldName { get; init; }
    public string? Quote { get; init; }
    public string? PageNumber { get; init; }
    public string? Url { get; init; }
    public DateOnly? AccessedDate { get; init; }
}
