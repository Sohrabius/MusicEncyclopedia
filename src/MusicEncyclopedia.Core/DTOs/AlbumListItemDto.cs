namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an album in list views (album listing, search results, etc.).
/// </summary>
public sealed class AlbumListItemDto
{
    public int AlbumId { get; init; }

    /// <summary>
    /// The referenced Entity row id — used to overlay localized titles (spec 7.3).
    /// Null when the query does not select it.
    /// </summary>
    public int? EntityId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; set; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public string? CategoryCode { get; init; }
    public string? CategoryName { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }
    public bool IsDeleted { get; init; }
    public IReadOnlyList<string> Genres { get; init; } = [];
    public IReadOnlyList<string> Moods { get; init; } = [];
    public IReadOnlyList<string> PrimaryArtists { get; init; } = [];
}
