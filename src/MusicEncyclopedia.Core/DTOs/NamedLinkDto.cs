namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a named link with a slug and display name.
/// Used for genres, moods, languages, countries, etc.
/// </summary>
public sealed class NamedLinkDto
{
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
}
