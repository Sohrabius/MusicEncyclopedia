namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides localized content values from the database Localization table.
/// Spec 7.3 — localization fallback chain: requested culture → base value → en → empty.
/// </summary>
public interface IContentLocalizationService
{
    /// <summary>
    /// Gets localized values for multiple fields of an entity in a single query.
    /// Returns, per field, the requested-culture value and the fallback-culture
    /// (English) value so callers can apply the spec 7.3 chain:
    /// requested → base column → en → empty.
    /// </summary>
    /// <param name="entityId">The entity id.</param>
    /// <param name="fieldNames">The field names to look up.</param>
    /// <param name="culture">The requested culture code (e.g., "fa", "fr").</param>
    /// <returns>
    /// A map of field name → (Requested, Fallback) localized values. Either value
    /// is <c>null</c> when no localization exists for that field/culture.
    /// </returns>
    Task<IReadOnlyDictionary<string, LocalizedFieldValues>> GetLocalizedValuesAsync(
        int entityId,
        IReadOnlyCollection<string> fieldNames,
        string culture,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets localized values for multiple fields of many entities in a single query.
    /// Returns, per entity id and field name, the requested-culture value and the
    /// fallback-culture (English) value.
    /// </summary>
    /// <param name="entityIds">The entity ids to look up.</param>
    /// <param name="fieldNames">The field names to look up.</param>
    /// <param name="culture">The requested culture code (e.g., "fa", "fr").</param>
    /// <returns>
    /// A map of entity id → field name → (Requested, Fallback) localized values.
    /// Entities or fields without any localization are absent from the result.
    /// </returns>
    Task<IReadOnlyDictionary<int, IReadOnlyDictionary<string, LocalizedFieldValues>>> GetLocalizedValuesAsync(
        IReadOnlyCollection<int> entityIds,
        IReadOnlyCollection<string> fieldNames,
        string culture,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Localized values for a single field in two cultures.
/// </summary>
public readonly record struct LocalizedFieldValues(string? Requested, string? Fallback);
