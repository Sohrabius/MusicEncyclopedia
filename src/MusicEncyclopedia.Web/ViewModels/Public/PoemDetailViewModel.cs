namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the poem detail page (spec 9.12).
/// The Poem property is dynamic because the Dapper-driven query returns
/// a shape that may evolve as the schema changes.
/// </summary>
public sealed class PoemDetailViewModel
{
    /// <summary>
    /// The full poem detail data.
    /// Expected properties: Title, OriginalTitle, EnglishTitle, Slug,
    /// CanonicalText, Book, Source, ExternalReferenceUrl, Copyright,
    /// OriginalPublicationDate, OriginalPublicationDatePrecision, Notes,
    /// Poet (object with FullName, Slug),
    /// Publication (object with Title, Slug),
    /// SungVersions (list of objects with Title, Slug, VocalStyleName, IsCanonical),
    /// Tracks (list of objects with Title, Slug, DurationSeconds),
    /// Media, Links, Citations, Tags, Aliases
    /// </summary>
    public required dynamic Poem { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
