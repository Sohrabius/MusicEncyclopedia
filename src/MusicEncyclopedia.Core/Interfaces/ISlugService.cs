namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides slug generation and validation functionality.
/// Slugs are lowercase, URL-safe, and unicode-friendly.
/// </summary>
public interface ISlugService
{
    /// <summary>
    /// Generates a URL-safe, lowercase slug from the given input string.
    /// Supports Unicode/Persian characters.
    /// </summary>
    string GenerateSlug(string input);

    /// <summary>
    /// Validates whether the given slug is properly formatted.
    /// </summary>
    bool IsValidSlug(string slug);

    /// <summary>
    /// Checks whether the given slug matches a reserved route.
    /// </summary>
    bool IsReservedRoute(string slug);
}
