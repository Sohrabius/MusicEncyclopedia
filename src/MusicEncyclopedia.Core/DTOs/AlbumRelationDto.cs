namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// A related album linked via AlbumRelation (spec 4.3), resolved in either
/// direction so both sides of the relation surface.
/// </summary>
public sealed class AlbumRelationDto
{
    public int AlbumRelationId { get; init; }

    public int RelatedAlbumId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public DateOnly? ReleaseDate { get; init; }
    public string? CoverUrl { get; init; }

    /// <summary>The relation type name (Reissue, Remaster, Follow-up, ...).</summary>
    public string RelationName { get; init; } = "";

    public string? RelationCode { get; init; }
}
