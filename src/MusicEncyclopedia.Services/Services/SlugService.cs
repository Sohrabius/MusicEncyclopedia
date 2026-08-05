using System.Text.RegularExpressions;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Provides slug generation, validation, and reserved route checking.
/// </summary>
public sealed class SlugService : ISlugService
{
    private static readonly Regex InvalidUrlCharsRegex = new(
        @"[^a-z0-9\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF\-]",
        RegexOptions.Compiled);

    private static readonly Regex ValidSlugRegex = new(
        @"^[a-z0-9\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF][a-z0-9\u0600-\u06FF\u0750-\u077F\u08A0-\u08FF\uFB50-\uFDFF\uFE70-\uFEFF\-]*$",
        RegexOptions.Compiled);

    private static readonly HashSet<string> ReservedRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "admin",
        "api",
        "auth",
        "search",
        "sitemap.xml",
        "robots.txt",
        "favicon.ico"
    };

    private const int MaxSlugLength = 255;

    /// <inheritdoc />
    public string GenerateSlug(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = input.ToLowerInvariant();

        // Replace spaces and underscores with hyphens
        slug = slug.Replace(' ', '-');
        slug = slug.Replace('_', '-');

        // Remove invalid URL characters, but preserve Unicode/Persian chars and hyphens
        slug = InvalidUrlCharsRegex.Replace(slug, "");

        // Collapse multiple consecutive hyphens into one
        slug = Regex.Replace(slug, @"-{2,}", "-");

        // Trim hyphens from start and end
        slug = slug.Trim('-');

        // Enforce maximum length
        if (slug.Length > MaxSlugLength)
        {
            slug = slug[..MaxSlugLength].Trim('-');
        }

        return slug;
    }

    /// <inheritdoc />
    public bool IsValidSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        if (slug.Length > MaxSlugLength)
            return false;

        return ValidSlugRegex.IsMatch(slug);
    }

    /// <inheritdoc />
    public bool IsReservedRoute(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        return ReservedRoutes.Contains(slug);
    }
}
