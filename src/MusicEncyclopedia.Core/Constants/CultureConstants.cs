namespace MusicEncyclopedia.Core.Constants;

/// <summary>
/// Defines supported cultures and fallback culture for the application.
/// Spec Section 7 — multilingual support: fa, en, ar, fr.
/// </summary>
public static class CultureConstants
{
    /// <summary>
    /// Default culture (Persian).
    /// </summary>
    public const string DefaultCulture = "fa";

    /// <summary>
    /// Language of the base content columns — the language the source data is
    /// authored in (the seeded content is English). Localized overlays are skipped
    /// for this culture because the base columns already match
    /// (spec 7.3 fallback chain: requested → base → en → empty).
    /// </summary>
    public const string BaseCulture = "en";

    /// <summary>
    /// Fallback culture (English). Used when a requested culture has no
    /// localized value for a field (spec 7.3 fallback chain: requested → base → en → empty).
    /// </summary>
    public const string FallbackCulture = "en";

    /// <summary>
    /// List of all supported cultures.
    /// </summary>
    public static readonly IReadOnlyList<string> SupportedCultures = new[] { "fa", "en", "ar", "fr" };

    /// <summary>
    /// Regex constraint used in route attributes to restrict the culture segment
    /// to the supported cultures.
    /// </summary>
    public const string RouteConstraint = "^(fa|en|ar|fr)$";

    /// <summary>
    /// Returns true if the given culture is supported.
    /// </summary>
    public static bool IsSupported(string? culture)
        => culture is not null && SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Returns true if the given culture renders right-to-left.
    /// </summary>
    public static bool IsRtl(string? culture)
        => string.Equals(culture, "fa", StringComparison.OrdinalIgnoreCase)
        || string.Equals(culture, "ar", StringComparison.OrdinalIgnoreCase);
}
