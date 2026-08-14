using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Services.Infrastructure;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Provides company query operations using Dapper.
/// </summary>
public sealed class CompanyQueryService : ICompanyQueryService
{
    private readonly string _connectionString;
    private readonly ILogger<CompanyQueryService> _logger;
    private readonly IContentLocalizationService _localizationService;
    private readonly ICacheService _cache;

    public CompanyQueryService(
        IConfiguration configuration,
        ILogger<CompanyQueryService> logger,
        IContentLocalizationService localizationService,
        ICacheService cache)
    {
        _localizationService = localizationService;
        _cache = cache;

        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    private IDbConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }

    /// <inheritdoc />
    public async Task<PagedResult<NamedLinkDto>> GetCompaniesAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        try
        {
            var cacheKey = CacheKeys.List("company", culture, page, pageSize, sort, q);

            var cached = await _cache.GetAsync<PagedResult<NamedLinkDto>>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadCompaniesCoreAsync(culture, page, pageSize, sort, q, cancellationToken);

            await _cache.SetAsync(cacheKey, result, CacheKeys.ListDuration, cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load company list for culture={Culture}, page={Page}", culture, page);
            return PagedResult<NamedLinkDto>.Create([], page, pageSize, 0);
        }
    }

    private async Task<PagedResult<NamedLinkDto>> LoadCompaniesCoreAsync(
        string culture,
        int page,
        int pageSize,
        string? sort,
        string? q,
        CancellationToken cancellationToken)
    {
        var whereClauses = new List<string> { "c.IsDeleted = 0" };
        var parameters = new DynamicParameters();

        if (!string.IsNullOrWhiteSpace(q))
        {
            whereClauses.Add("(c.Name LIKE @Q OR c.OriginalName LIKE @Q OR c.EnglishName LIKE @Q)");
            parameters.Add("Q", $"%{q}%");
        }

        var whereSql = string.Join(" AND ", whereClauses);

        var orderBy = sort?.ToLowerInvariant() switch
        {
            "name" => "c.Name ASC",
            "createdat" => "c.CreatedAt DESC",
            _ => "c.Name ASC"
        };

        var countSql = $"""
            SELECT COUNT(1)
            FROM Company AS c
            WHERE {whereSql}
            """;

        var dataSql = $"""
            SELECT
                c.Slug,
                c.Name
            FROM Company AS c
            WHERE {whereSql}
            ORDER BY {orderBy}
            {SqlDialect.Pagination()}
            """;

        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("PageSize", pageSize);

        using var connection = CreateConnection();
        connection.Open();

        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, parameters);
        var items = await connection.QueryAsync<NamedLinkDto>(dataSql, parameters);

        return PagedResult<NamedLinkDto>.Create(items.AsList(), page, pageSize, totalItems);
    }

    /// <inheritdoc />
    public async Task<object?> GetCompanyBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = CacheKeys.Detail("company", culture, slug);

            var cached = await _cache.GetAsync<object>(cacheKey, cancellationToken);
            if (cached is not null)
                return cached;

            var result = await LoadCompanyDetailCoreAsync(slug, culture, cancellationToken);
            if (result is not null)
            {
                await _cache.SetAsync(cacheKey, result, CacheKeys.DetailDuration, cancellationToken);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load company by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private async Task<object?> LoadCompanyDetailCoreAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken)
    {
        const string companySql = """
            SELECT
                c.CompanyId,
                c.EntityId,
                c.Slug,
                c.Name,
                c.OriginalName,
                c.EnglishName,
                ct.Name AS CompanyType,
                co.Name AS CountryName,
                c.Website,
                c.History
            FROM Company AS c
            LEFT JOIN CompanyType AS ct ON ct.CompanyTypeId = c.CompanyTypeId
            LEFT JOIN Country AS co ON co.CountryId = c.CountryId
            WHERE c.Slug = @Slug
              AND c.IsDeleted = 0
            """;

        try
        {
            using var connection = CreateConnection();
            connection.Open();

            var company = await connection.QuerySingleOrDefaultAsync<dynamic>(
                companySql, new { Slug = slug });

            if (company is null)
                return null;

            var entityId = (int)company.EntityId;

            IReadOnlyList<MediaDto> media;
            IReadOnlyList<EntityLinkDto> links;
            IReadOnlyList<AliasDto> aliases;
            IReadOnlyList<TagDto> tags;
            IReadOnlyList<CitationDto> citations;

            // Spec 7.3: fetch localized text (requested + English) in parallel with
            // the detail sub-queries, using its own connection.
            Task<IReadOnlyDictionary<string, LocalizedFieldValues>>? localizationTask = null;
            if (LocalizationHelper.ShouldLocalize(culture))
            {
                localizationTask = _localizationService.GetLocalizedValuesAsync(
                    entityId, LocalizedCompanyFields, culture, cancellationToken);
            }

            {
                var mediaTask = GetEntityMediaAsync(connection, entityId, cancellationToken);
                var linksTask = GetEntityLinksAsync(connection, entityId, cancellationToken);
                var aliasesTask = GetEntityAliasesAsync(connection, entityId, cancellationToken);
                var tagsTask = GetEntityTagsAsync(connection, entityId, cancellationToken);
                var citationsTask = GetEntityCitationsAsync(connection, entityId, cancellationToken);

                var tasks = new List<Task> { mediaTask, linksTask, aliasesTask, tagsTask, citationsTask };
                if (localizationTask is not null)
                    tasks.Add(localizationTask);

                await Task.WhenAll(tasks);

                media = mediaTask.Result;
                links = linksTask.Result;
                aliases = aliasesTask.Result;
                tags = tagsTask.Result;
                citations = citationsTask.Result;
            }

            var localized = localizationTask is null
                ? (IReadOnlyDictionary<string, LocalizedFieldValues>)
                    new Dictionary<string, LocalizedFieldValues>(StringComparer.OrdinalIgnoreCase)
                : await localizationTask;

            return new
            {
                CompanyId = (int)company.CompanyId,
                EntityId = entityId,
                Slug = (string)company.Slug,
                Name = LocalizationHelper.Pick(localized, "Name", (string)company.Name),
                OriginalName = LocalizationHelper.Pick(localized, "OriginalName", (string?)company.OriginalName),
                EnglishName = LocalizationHelper.Pick(localized, "EnglishName", (string?)company.EnglishName),
                CompanyType = (string?)company.CompanyType,
                CountryName = (string?)company.CountryName,
                Website = (string?)company.Website,
                History = LocalizationHelper.Pick(localized, "History", (string?)company.History),
                Media = media,
                Links = links,
                Aliases = aliases,
                Tags = tags,
                Citations = citations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load company by slug={Slug}, culture={Culture}", slug, culture);
            return null;
        }
    }

    private static readonly string[] LocalizedCompanyFields =
        ["Name", "OriginalName", "EnglishName", "History"];

    private static async Task<IReadOnlyList<MediaDto>> GetEntityMediaAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                m.MediaId,
                m.Url,
                m.ThumbnailUrl300 AS ThumbnailUrl,
                mt.Name AS MediaType,
                mrt.Name AS MediaRole,
                ma.IsPrimary,
                m.Width,
                m.Height
            FROM MediaAssignment ma
            INNER JOIN Media m ON m.MediaId = ma.MediaId
            INNER JOIN MediaType mt ON mt.MediaTypeId = m.MediaTypeId
            LEFT JOIN MediaRoleType mrt ON mrt.MediaRoleTypeId = ma.MediaRoleTypeId
            WHERE ma.EntityId = @EntityId
              AND m.IsDeleted = 0
            ORDER BY ma.IsPrimary DESC, m.MediaId
            """;
        var results = await connection.QueryAsync<MediaDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<EntityLinkDto>> GetEntityLinksAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                el.EntityLinkId,
                el.Url,
                lt.Name AS LinkType,
                el.Title
            FROM EntityLink el
            LEFT JOIN LinkType lt ON lt.LinkTypeId = el.LinkTypeId
            WHERE el.EntityId = @EntityId
              AND el.IsDeleted = 0
            ORDER BY el.EntityLinkId
            """;
        var results = await connection.QueryAsync<EntityLinkDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<AliasDto>> GetEntityAliasesAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.AliasId,
                a.AliasName,
                at.Name AS AliasType,
                l.Code AS Language,
                a.IsPrimary,
                a.Notes
            FROM Alias a
            LEFT JOIN AliasType at ON at.AliasTypeId = a.AliasTypeId
            LEFT JOIN Language l ON l.LanguageId = a.LanguageId
            WHERE a.EntityId = @EntityId
            ORDER BY a.IsPrimary DESC, a.AliasId
            """;
        var results = await connection.QueryAsync<AliasDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<TagDto>> GetEntityTagsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT t.TagId, t.Name, t.Slug
            FROM TagAssignment ta
            INNER JOIN Tag t ON t.TagId = ta.TagId
            WHERE ta.EntityId = @EntityId
              AND t.IsDeleted = 0
            ORDER BY t.Name
            """;
        var results = await connection.QueryAsync<TagDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<CitationDto>> GetEntityCitationsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                c.CitationId,
                c.SourceId,
                s.Title AS SourceName,
                s.Slug AS SourceSlug,
                c.FieldName,
                c.Quote,
                c.PageNumber,
                c.Url,
                c.AccessedDate
            FROM Citation c
            LEFT JOIN Source s ON s.SourceId = c.SourceId
            WHERE c.EntityId = @EntityId
            ORDER BY c.CitationId
            """;
        var results = await connection.QueryAsync<CitationDto>(sql, new { EntityId = entityId });
        return results.AsList();
    }
}
