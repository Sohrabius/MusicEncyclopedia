namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents an album in list views (album listing, search results, etc.).
/// </summary>
public sealed class AlbumListItemDto
{
    public int AlbumId { get; init; }
    public string Slug { get; init; } = "";
    public string Title { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public string? CategoryName { get; init; }
    public DateOnly? ReleaseDate { get; init; }
    public int? DurationSeconds { get; init; }
    public string? CoverUrl { get; init; }
    public IReadOnlyList<string> Genres { get; init; } = [];
    public IReadOnlyList<string> Moods { get; init; } = [];
    public IReadOnlyList<string> PrimaryArtists { get; init; } = [];
}
