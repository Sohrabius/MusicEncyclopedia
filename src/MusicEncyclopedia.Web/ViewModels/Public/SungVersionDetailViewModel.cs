namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the sung version detail page (spec 9.13).
/// The SungVersion property is dynamic because the Dapper-driven query
/// returns a shape that may evolve as the schema changes.
/// </summary>
public sealed class SungVersionDetailViewModel
{
    /// <summary>
    /// The full sung version detail data.
    /// Expected properties: Title, Slug, Text, Notes, IsCanonical,
    /// VocalStyleName,
    /// Poem (object with Title, Slug, CanonicalText),
    /// Poet (object with FullName, Slug),
    /// Tracks (list of objects with Title, Slug, DurationSeconds),
    /// Media, Links, Citations, Tags
    /// </summary>
    public required dynamic SungVersion { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
