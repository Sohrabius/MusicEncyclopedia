namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// A related track linked via TrackRelation (spec 4.3), resolved in either
/// direction so both sides of the relation surface.
/// </summary>
public sealed class TrackRelationDto
{
    public int TrackRelationId { get; init; }

    public int RelatedTrackId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public int? DurationSeconds { get; init; }

    /// <summary>The relation type name (Cover, Remix, Live Version, ...).</summary>
    public string RelationName { get; init; } = "";

    public string? RelationCode { get; init; }
}
