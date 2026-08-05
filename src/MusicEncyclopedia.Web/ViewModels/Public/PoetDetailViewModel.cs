namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the poet detail page (spec 9.11).
/// A poet is a Person with PersonKind = "poet".
/// The Poet property is dynamic (same pattern as PersonDetailViewModel).
/// Expected additional sections: Poems, Publications, SungVersions.
/// </summary>
public sealed class PoetDetailViewModel
{
    /// <summary>
    /// The full poet (Person) detail data.
    /// Properties expected: FullName, OriginalName, EnglishName, PersonKind,
    /// Nationality, BirthDate, DeathDate, BirthPlace, DeathPlace,
    /// Biography, ImageUrl,
    /// Poems (list of objects with Title, Slug),
    /// Publications (list of objects with Title, Slug, PublicationDate),
    /// SungVersions (list of objects with Title, Slug, VocalStyleName),
    /// Instruments, Roles, Media, Links, Aliases, Tags, Citations, Timeline
    /// </summary>
    public required dynamic Poet { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
