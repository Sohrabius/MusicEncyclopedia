namespace MusicEncyclopedia.Core.Constants;

/// <summary>
/// Defines supported cultures and fallback culture for the application.
/// </summary>
public static class CultureConstants
{
    /// <summary>
    /// Default culture (Persian).
    /// </summary>
    public const string DefaultCulture = "fa";

    /// <summary>
    /// Fallback culture (English).
    /// </summary>
    public const string FallbackCulture = "fa";

    /// <summary>
    /// List of all supported cultures.
    /// </summary>
    public static readonly IReadOnlyList<string> SupportedCultures = new[] { "fa" };

    /// <summary>
    /// Returns true if the given culture is supported.
    /// </summary>
    public static bool IsSupported(string? culture)
        => culture is not null && SupportedCultures.Contains(culture, StringComparer.OrdinalIgnoreCase);
}
