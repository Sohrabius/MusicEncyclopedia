namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the person detail page (9.6).
/// The Person property is dynamic because the service returns object?
/// (the concrete PersonDetailDto lives in the services layer).
/// </summary>
public sealed class PersonDetailViewModel
{
    /// <summary>
    /// The full person detail data from the service.
    /// Properties expected: FullName, OriginalName, EnglishName, PersonKind,
    /// Nationality, BirthDate, DeathDate, BirthPlace, DeathPlace,
    /// Biography, ImageUrl, Instruments, Roles, Timeline, Media,
    /// Links, Aliases, Tags, Citations
    /// </summary>
    public required dynamic Person { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
