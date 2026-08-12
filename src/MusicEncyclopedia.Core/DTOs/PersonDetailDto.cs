namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// The full detail of a person for its dedicated page (spec 9.6).
/// Carries the career timeline, discography and track contributions (each with
/// filter option lists), plus the classic entity sections.
/// </summary>
public sealed class PersonDetailDto
{
    public int PersonId { get; init; }
    public int EntityId { get; init; }
    public string Slug { get; init; } = "";
    public string FullName { get; set; } = "";
    public string? OriginalName { get; set; }
    public string? EnglishName { get; set; }
    public string? PersonKind { get; init; }
    public string? Nationality { get; init; }
    public DateOnly? BirthDate { get; init; }
    public string? BirthDatePrecision { get; init; }
    public DateOnly? DeathDate { get; init; }
    public string? DeathDatePrecision { get; init; }
    public string? Biography { get; set; }
    public string? ImageUrl { get; init; }

    // ── Phase 4 additions ──
    public IReadOnlyList<TimelineEntryDto> Timeline { get; init; } = [];
    public IReadOnlyList<PersonAlbumDto> Albums { get; init; } = [];
    public IReadOnlyList<TrackContributionDto> TrackContributions { get; init; } = [];

    // Distinct option lists for the discography / contributions filter forms.
    public IReadOnlyList<string> AlbumCategories { get; init; } = [];
    public IReadOnlyList<int> AlbumYears { get; init; } = [];
    public IReadOnlyList<string> AlbumRoles { get; init; } = [];
    public IReadOnlyList<string> AlbumInstruments { get; init; } = [];
    public IReadOnlyList<string> ContributionRoles { get; init; } = [];
    public IReadOnlyList<string> ContributionInstruments { get; init; } = [];
    public IReadOnlyList<string> ContributionAlbums { get; init; } = [];
    public IReadOnlyList<string> ContributionGenres { get; init; } = [];

    // ── Classic sections (reused by the person page) ──
    public IReadOnlyList<NamedLinkDto> Instruments { get; init; } = [];
    public IReadOnlyList<NamedLinkDto> Roles { get; init; } = [];
    public IReadOnlyList<MediaDto> Media { get; init; } = [];
    public IReadOnlyList<EntityLinkDto> Links { get; init; } = [];
    public IReadOnlyList<AliasDto> Aliases { get; init; } = [];
    public IReadOnlyList<TagDto> Tags { get; init; } = [];
    public IReadOnlyList<CitationDto> Citations { get; init; } = [];
}
