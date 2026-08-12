namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents the full detail of an album for its dedicated page.
/// </summary>
public sealed class AlbumDetailDto
{
    public int AlbumId { get; init; }
    public int EntityId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; set; } = "";
    public string? OriginalTitle { get; set; }
    public string? EnglishTitle { get; set; }
    public string? Description { get; set; }
    public string? CategoryName { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public DateOnly? RecordingStartDate { get; init; }
    public DateOnly? RecordingEndDate { get; init; }
    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }
    public string? CopyrightNotice { get; init; }

    public IReadOnlyList<AlbumTrackDto> Tracks { get; init; } = [];
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Genres { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Moods { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Languages { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Countries { get; init; } = [];
    public IReadOnlyList<AlbumCompanyDto> Companies { get; init; } = [];
    public IReadOnlyList<IdentifierDto> Identifiers { get; init; } = [];
    public IReadOnlyList<MediaDto> Media { get; init; } = [];
    public IReadOnlyList<EntityLinkDto> Links { get; init; } = [];
    public IReadOnlyList<AliasDto> Aliases { get; init; } = [];
    public IReadOnlyList<TagDto> Tags { get; init; } = [];
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
    public IReadOnlyList<AwardAssignmentDto> Awards { get; init; } = [];
    public IReadOnlyList<CertificationAssignmentDto> Certifications { get; init; } = [];
    public IReadOnlyList<ChartEntryDto> ChartEntries { get; init; } = [];
}
