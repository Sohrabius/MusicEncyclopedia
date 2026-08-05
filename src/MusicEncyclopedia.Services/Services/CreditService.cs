using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Provides credit-related query operations using Dapper.
/// </summary>
public sealed class CreditService : ICreditService
{
    private readonly string _connectionString;
    private readonly ILogger<CreditService> _logger;

    public CreditService(
        IConfiguration configuration,
        ILogger<CreditService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CreditDto>> GetCreditsAsync(
        int entityId,
        string entityTypeCode,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                c.CreditId,
                c.PersonId,
                p.FullName AS PersonFullName,
                p.Slug AS PersonSlug,
                c.CompanyId,
                co.Name AS CompanyName,
                co.Slug AS CompanySlug,
                cr.Name AS RoleName,
                cr.Code AS RoleCode,
                cr.DisplayOrder AS RoleDisplayOrder,
                i.Name AS InstrumentName,
                c.DisplayOrder,
                c.IsPrimary,
                c.Notes
            FROM Credit AS c
            INNER JOIN Entity AS e ON e.EntityId = c.EntityId
            INNER JOIN EntityType AS et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN CreditRole AS cr ON cr.CreditRoleId = c.CreditRoleId
            LEFT JOIN Person AS p ON p.PersonId = c.PersonId
            LEFT JOIN Company AS co ON co.CompanyId = c.CompanyId
            LEFT JOIN Instrument AS i ON i.InstrumentId = c.InstrumentId
            WHERE c.EntityId = @EntityId
              AND et.Code = @EntityTypeCode
              AND c.IsDeleted = 0
            ORDER BY cr.DisplayOrder, c.DisplayOrder, p.FullName, co.Name
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<CreditDto>(
                sql,
                new { EntityId = entityId, EntityTypeCode = entityTypeCode });

            return results.AsList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load credits for EntityId={EntityId}, EntityTypeCode={EntityTypeCode}",
                entityId, entityTypeCode);
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CreditGroupDto>> GetGroupedCreditsAsync(
        int entityId,
        string entityTypeCode,
        CancellationToken cancellationToken = default)
    {
        var credits = await GetCreditsAsync(entityId, entityTypeCode, cancellationToken);

        var grouped = credits
            .GroupBy(c => c.RoleName)
            .Select(g => new CreditGroupDto
            {
                RoleName = g.Key,
                Credits = g.OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.PersonFullName)
                    .ThenBy(c => c.CompanyName)
                    .ToList()
            })
            .ToList();

        return grouped;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MusicianCreditDto>> GetMusicianCreditsAsync(
        int entityId,
        string entityTypeCode,
        CancellationToken cancellationToken = default)
    {
        // Use the dedicated musician view for efficiency
        const string sql = """
            SELECT
                c.CreditId,
                c.PersonId,
                p.FullName AS PersonFullName,
                p.Slug AS PersonSlug,
                i.Name AS InstrumentName,
                i.InstrumentId,
                c.Notes
            FROM Credit AS c
            INNER JOIN Entity AS e ON e.EntityId = c.EntityId
            INNER JOIN EntityType AS et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN CreditRole AS cr ON cr.CreditRoleId = c.CreditRoleId
            INNER JOIN Person AS p ON p.PersonId = c.PersonId
            LEFT JOIN Instrument AS i ON i.InstrumentId = c.InstrumentId
            WHERE c.EntityId = @EntityId
              AND et.Code = @EntityTypeCode
              AND cr.Code = 'MUSICIAN'
              AND c.IsDeleted = 0
            ORDER BY c.DisplayOrder, p.FullName, i.Name
            """;

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var results = await connection.QueryAsync<MusicianCreditDto>(
                sql,
                new { EntityId = entityId, EntityTypeCode = entityTypeCode });

            return results.AsList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to load musician credits for EntityId={EntityId}, EntityTypeCode={EntityTypeCode}",
                entityId, entityTypeCode);
            return [];
        }
    }
}
