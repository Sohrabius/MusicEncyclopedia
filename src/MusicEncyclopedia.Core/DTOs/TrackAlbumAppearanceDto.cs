namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an album that a track appears on, with album-specific metadata.
/// </summary>
public sealed class TrackAlbumAppearanceDto
{
    public int AlbumId { get; init; }
    public string AlbumTitle { get; init; } = "";
    public string AlbumSlug { get; init; } = "";
    public string? CategoryName { get; init; }
    public string? CoverUrl { get; init; }
    public int DiscNumber { get; init; }
    public int TrackNumber { get; init; }
    public int SequenceNumber { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? DurationSecondsOverride { get; init; }
    public string? TrackTitleOverride { get; init; }
}
