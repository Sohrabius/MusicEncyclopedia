using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Infrastructure;

/// <summary>
/// Shared helpers for applying the spec 7.3 localization fallback chain
/// (requested culture → base column → en → empty) to query results.
/// </summary>
public static class LocalizationHelper
{
    /// <summary>
    /// Returns true when localized overlays should be applied for the culture —
    /// i.e. the culture is supported and is not the base language the content
    /// columns are authored in.
    /// </summary>
    public static bool ShouldLocalize(string? culture)
        => !string.IsNullOrWhiteSpace(culture)
           && !string.Equals(culture, CultureConstants.BaseCulture, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Picks the display value for a field: the requested culture wins; otherwise
    /// the base column value is kept; only when the base is empty does it fall back
    /// to the English localization.
    /// </summary>
    public static string Pick(
        IReadOnlyDictionary<string, LocalizedFieldValues> localized,
        string fieldName,
        string? baseValue)
    {
        if (localized.TryGetValue(fieldName, out var values))
        {
            if (!string.IsNullOrWhiteSpace(values.Requested))
                return values.Requested!;
            if (string.IsNullOrWhiteSpace(baseValue) && !string.IsNullOrWhiteSpace(values.Fallback))
                return values.Fallback!;
        }
        return baseValue ?? string.Empty;
    }
}
