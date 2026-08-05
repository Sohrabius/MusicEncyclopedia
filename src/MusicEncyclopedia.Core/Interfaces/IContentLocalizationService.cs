namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides localized content values from the database Localization table.
/// </summary>
public interface IContentLocalizationService
{
    /// <summary>
    /// Gets a localized value for the specified entity, field, and culture.
    /// Falls back to the provided fallback value if no localization exists.
    /// </summary>
    Task<string> GetLocalizedValueAsync(
        int entityId,
        string fieldName,
        string culture,
        string fallback,
        CancellationToken cancellationToken = default);
}
