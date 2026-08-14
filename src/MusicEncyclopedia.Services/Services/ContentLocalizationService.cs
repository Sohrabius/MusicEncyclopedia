using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Provides localized content values from the database Localization table.
/// Uses the Language table (LanguageId FK → Language.Code) to resolve cultures,
/// and supports batched lookups.
/// </summary>
public sealed class ContentLocalizationService : IContentLocalizationService
{
    private readonly string _connectionString;
    private readonly ILogger<ContentLocalizationService> _logger;

    public ContentLocalizationService(
        IConfiguration configuration,
        ILogger<ContentLocalizationService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    private IDbConnection CreateConnection()
    {
        return new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<string, LocalizedFieldValues>> GetLocalizedValuesAsync(
        int entityId,
        IReadOnlyCollection<string> fieldNames,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, LocalizedFieldValues>(StringComparer.OrdinalIgnoreCase);

        if (fieldNames.Count == 0)
            return result;

        foreach (var field in fieldNames)
        {
            result[field] = default;
        }

        // Both the requested culture and the fallback culture (en) in one query.
        const string sql = """
            SELECT
                loc.FieldName,
                l.Code AS Culture,
                loc.LocalizedText
            FROM Localization loc
            INNER JOIN Language l ON l.LanguageId = loc.LanguageId
            WHERE loc.EntityId = @EntityId
              AND l.Code IN (@RequestedCulture, @FallbackCulture)
            ORDER BY loc.LocalizationId
            """;

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var rows = await connection.QueryAsync<(string FieldName, string Culture, string LocalizedText)>(
                sql,
                new
                {
                    EntityId = entityId,
                    RequestedCulture = culture,
                    FallbackCulture = CultureConstants.FallbackCulture
                });

            foreach (var row in rows)
            {
                if (!result.TryGetValue(row.FieldName, out var values))
                    continue;

                if (string.Equals(row.Culture, culture, StringComparison.OrdinalIgnoreCase))
                    result[row.FieldName] = values with { Requested = row.LocalizedText };
                else
                    result[row.FieldName] = values with { Fallback = row.LocalizedText };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to retrieve localizations for EntityId={EntityId}, Fields={Fields}, Culture={Culture}",
                entityId, string.Join(",", fieldNames), culture);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<int, IReadOnlyDictionary<string, LocalizedFieldValues>>> GetLocalizedValuesAsync(
        IReadOnlyCollection<int> entityIds,
        IReadOnlyCollection<string> fieldNames,
        string culture,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<int, IReadOnlyDictionary<string, LocalizedFieldValues>>();

        if (entityIds.Count == 0 || fieldNames.Count == 0)
            return result;

        var distinctIds = entityIds.Distinct().ToArray();

        // Both the requested culture and the fallback culture (en) in one query;
        // Dapper expands IN @EntityIds into parameterized placeholders on both providers.
        const string sql = """
            SELECT
                loc.EntityId,
                loc.FieldName,
                l.Code AS Culture,
                loc.LocalizedText
            FROM Localization loc
            INNER JOIN Language l ON l.LanguageId = loc.LanguageId
            WHERE loc.EntityId IN @EntityIds
              AND l.Code IN (@RequestedCulture, @FallbackCulture)
            ORDER BY loc.LocalizationId
            """;

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var rows = await connection.QueryAsync<(int EntityId, string FieldName, string Culture, string LocalizedText)>(
                sql,
                new
                {
                    EntityIds = distinctIds,
                    RequestedCulture = culture,
                    FallbackCulture = CultureConstants.FallbackCulture
                });

            var byEntity = new Dictionary<int, Dictionary<string, LocalizedFieldValues>>(distinctIds.Length);
            foreach (var id in distinctIds)
            {
                byEntity[id] = new Dictionary<string, LocalizedFieldValues>(StringComparer.OrdinalIgnoreCase);
            }

            foreach (var row in rows)
            {
                if (!byEntity.TryGetValue(row.EntityId, out var fields))
                    continue;

                if (!fields.TryGetValue(row.FieldName, out var values))
                {
                    values = default;
                    fields[row.FieldName] = values;
                }

                if (string.Equals(row.Culture, culture, StringComparison.OrdinalIgnoreCase))
                    fields[row.FieldName] = values with { Requested = row.LocalizedText };
                else
                    fields[row.FieldName] = values with { Fallback = row.LocalizedText };
            }

            foreach (var (id, fields) in byEntity)
            {
                if (fields.Count > 0)
                    result[id] = fields;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to retrieve localizations for EntityIds={EntityIds}, Fields={Fields}, Culture={Culture}",
                string.Join(",", distinctIds), string.Join(",", fieldNames), culture);
        }

        return result;
    }
}
