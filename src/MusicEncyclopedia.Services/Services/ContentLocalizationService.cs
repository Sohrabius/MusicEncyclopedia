using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Provides localized content values from the database Localization table.
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

    /// <inheritdoc />
    public async Task<string> GetLocalizedValueAsync(
        int entityId,
        string fieldName,
        string culture,
        string fallback,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT TOP 1 LocalizedText
            FROM Localization
            WHERE EntityId = @EntityId
              AND FieldName = @FieldName
              AND LanguageCode = @Culture
              AND IsDeleted = 0
            ORDER BY IsPrimary DESC, LocalizationId ASC
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var localized = await connection.QuerySingleOrDefaultAsync<string>(
                sql,
                new { EntityId = entityId, FieldName = fieldName, Culture = culture });

            if (!string.IsNullOrWhiteSpace(localized))
                return localized!;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to retrieve localization for EntityId={EntityId}, Field={Field}, Culture={Culture}",
                entityId, fieldName, culture);
        }

        // Fallback logic
        if (!string.IsNullOrWhiteSpace(fallback))
        {
            _logger.LogDebug(
                "Localization fallback used for EntityId={EntityId}, Field={Field}, Culture={Culture}, Fallback={Fallback}",
                entityId, fieldName, culture, fallback);
            return fallback;
        }

        return string.Empty;
    }
}
