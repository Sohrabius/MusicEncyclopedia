namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a track within an album's tracklist, including overrides.
/// </summary>
public sealed class AlbumTrackDto
{
    public int AlbumTrackId { get; init; }
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? TrackTitleOverride { get; init; }
    public int? DurationSecondsOverride { get; init; }
    public int? DurationSeconds { get; init; }
    public int DiscNumber { get; init; }
    public int TrackNumber { get; init; }
    public int SequenceNumber { get; init; }
    public bool IsBonus { get; init; }
    public bool IsHidden { get; init; }
    public IReadOnlyList<string> PrimaryArtists { get; init; } = [];
}
