namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents the full detail of a track for its dedicated page.
/// </summary>
public sealed class TrackDetailDto
{
    public int TrackId { get; init; }
    public int EntityId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public int? DurationSeconds { get; init; }
    public bool IsInstrumental { get; init; }
    public bool IsExplicit { get; init; }
    public string? Isrc { get; init; }
    public short? Bpm { get; init; }
    public string? MusicalKeyName { get; init; }
    public string? VocalStyleName { get; init; }
    public string? LyricsAvailabilityName { get; init; }

    public IReadOnlyList<TrackAlbumAppearanceDto> Albums { get; init; } = [];
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];
    public IReadOnlyList<MusicianCreditDto> Musicians { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Genres { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Moods { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Instruments { get; init; } = [];
    public IReadOnlyList<MediaDto> Media { get; init; } = [];
    public IReadOnlyList<EntityLinkDto> Links { get; init; } = [];
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
    public IReadOnlyList<TagDto> Tags { get; init; } = [];
    public IReadOnlyList<AliasDto> Aliases { get; init; } = [];
    public IReadOnlyList<AwardAssignmentDto> Awards { get; init; } = [];
    public IReadOnlyList<CertificationAssignmentDto> Certifications { get; init; } = [];
    public IReadOnlyList<ChartEntryDto> ChartEntries { get; init; } = [];
}
