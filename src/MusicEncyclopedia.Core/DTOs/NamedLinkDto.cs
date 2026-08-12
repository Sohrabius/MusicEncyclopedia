namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a named link with a slug and display name.
/// Used for genres, moods, languages, countries, etc.
/// </summary>
public sealed class NamedLinkDto
{
    public string Slug { get; init; } = "";

    /// <summary>
    /// The referenced Entity row id — used to overlay localized names (spec 7.3).
    /// Null when the query does not select it (genres, moods, etc.).
    /// </summary>
    public int? EntityId { get; init; }
    public string Name { get; set; } = "";
}
