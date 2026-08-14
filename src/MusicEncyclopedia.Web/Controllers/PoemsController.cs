using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for poem listing and detail pages.
/// Routes: /{culture}/poems and /{culture}/poems/{slug}
/// Spec references: 8.1 (routes), 9.12 (detail page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/poems")]
public sealed class PoemsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<PoemsController> _logger;

    public PoemsController(
        IDbConnection db,
        ILogger<PoemsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Poem listing page with pagination.
    /// Uses Dapper directly per spec.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Poem list requested: culture={Culture}, page={Page}", culture, page);

        page = Math.Max(1, page);
        var pageSize = 24;
        var offset = (page - 1) * pageSize;

        try
        {
            var countSql = """
                SELECT COUNT(1)
                FROM Poem p
                WHERE p.IsDeleted = 0
                """;

            var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

            var dataSql = $"""
                SELECT
                    p.Slug,
                    p.Title,
                    pers.FullName AS PoetName,
                    pers.Slug AS PoetSlug,
                    pub.Title AS PublicationName,
                    pub.Slug AS PublicationSlug
                FROM Poem p
                LEFT JOIN Person pers ON pers.PersonId = p.PersonId AND pers.IsDeleted = 0
                LEFT JOIN Publication pub ON pub.PublicationId = p.PublicationId AND pub.IsDeleted = 0
                WHERE p.IsDeleted = 0
                ORDER BY p.CreatedAt DESC
                {SqlDialect.Pagination()}
                """;

            var rows = await _db.QueryAsync<PoemListViewModel.PoemRow>(dataSql, new { Offset = offset, PageSize = pageSize });

            var totalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 0;

            var viewModel = new PoemListViewModel
            {
                Items = new PoemListViewModel.PagedResultViewModel
                {
                    Items = rows.AsList(),
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = totalPages
                },
                Page = page,
                Culture = culture
            };

            ViewData["Title"] = "Poems";
            ViewData["MetaDescription"] = "Browse the complete catalog of poems.";
            ViewData["Robots"] = "index, follow";
            ViewData["OgType"] = "website";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load poem list for culture={Culture}, page={Page}", culture, page);

            ViewData["Title"] = "Poems";
            return View(new PoemListViewModel
            {
                Items = new PoemListViewModel.PagedResultViewModel
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                },
                Page = page,
                Culture = culture
            });
        }
    }

    /// <summary>
    /// Poem detail page per spec 9.12.
    /// Displays canonical text, poet, publication, sung versions, tracks.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Poem detail requested: culture={Culture}, slug={Slug}", culture, slug);

        try
        {
            // Load poem
            var poemSql = """
                SELECT
                    p.PoemId,
                    p.EntityId,
                    p.Slug,
                    p.Title,
                    p.OriginalTitle,
                    p.EnglishTitle,
                    p.CanonicalText,
                    p.Book,
                    p.Source,
                    p.ExternalReferenceUrl,
                    p.Copyright,
                    p.OriginalPublicationDate,
                    p.OriginalPublicationDatePrecision,
                    p.Notes,
                    pers.FullName AS PoetName,
                    pers.Slug AS PoetSlug,
                    pub.Title AS PublicationTitle,
                    pub.Slug AS PublicationSlug
                FROM Poem p
                LEFT JOIN Person pers ON pers.PersonId = p.PersonId AND pers.IsDeleted = 0
                LEFT JOIN Publication pub ON pub.PublicationId = p.PublicationId AND pub.IsDeleted = 0
                WHERE p.Slug = @Slug AND p.IsDeleted = 0
                """;

            var poem = await _db.QuerySingleOrDefaultAsync(poemSql, new { Slug = slug });

            if (poem is null)
            {
                _logger.LogWarning("Poem not found: slug={Slug}, culture={Culture}", slug, culture);
                return NotFound();
            }

            var poemId = (int)((dynamic)poem).PoemId;
            var entityId = (int)((dynamic)poem).EntityId;

            // Load related data sequentially (shared scoped connection)
            var sungVersions = await GetSungVersionsAsync(_db, poemId, cancellationToken);
            var tracks = await GetTracksForPoemAsync(_db, poemId, cancellationToken);
            var media = await GetEntityMediaAsync(_db, entityId, cancellationToken);
            var links = await GetEntityLinksAsync(_db, entityId, cancellationToken);
            var citations = await GetEntityCitationsAsync(_db, entityId, cancellationToken);
            var tags = await GetEntityTagsAsync(_db, entityId, cancellationToken);
            var aliases = await GetEntityAliasesAsync(_db, entityId, cancellationToken);

            // Build a dynamic object with all data
            var detail = new
            {
                poem.Slug,
                poem.Title,
                poem.OriginalTitle,
                poem.EnglishTitle,
                poem.CanonicalText,
                poem.Book,
                poem.Source,
                poem.ExternalReferenceUrl,
                poem.Copyright,
                poem.OriginalPublicationDate,
                poem.OriginalPublicationDatePrecision,
                poem.Notes,
                Poet = poem.PoetName is not null
                    ? new { FullName = (string)poem.PoetName, Slug = (string)poem.PoetSlug }
                    : null,
                Publication = poem.PublicationTitle is not null
                    ? new { Title = (string)poem.PublicationTitle, Slug = (string)poem.PublicationSlug }
                    : null,
                SungVersions = sungVersions,
                Tracks = tracks,
                Media = media,
                Links = links,
                Citations = citations,
                Tags = tags,
                Aliases = aliases
            };

            var viewModel = new PoemDetailViewModel
            {
                Poem = detail,
                Culture = culture
            };

            ViewData["Title"] = poem.Title ?? "Poem";
            ViewData["MetaDescription"] = $"Poem: {poem.Title}";
            ViewData["Robots"] = "index, follow";
            ViewData["OgType"] = "article";
            ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/poems/{slug}";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load poem detail: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────────

    private static async Task<IReadOnlyList<dynamic>> GetSungVersionsAsync(
        IDbConnection connection,
        int poemId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                sv.Slug,
                sv.Title,
                vs.Name AS VocalStyleName,
                sv.IsCanonical
            FROM SungVersion sv
            LEFT JOIN VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
            WHERE sv.PoemId = @PoemId AND sv.IsDeleted = 0
            ORDER BY sv.IsCanonical DESC, sv.Title
            """;
        var results = await connection.QueryAsync(sql, new { PoemId = poemId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetTracksForPoemAsync(
        IDbConnection connection,
        int poemId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT DISTINCT
                t.Slug,
                t.Title,
                t.DurationSeconds
            FROM TrackSungVersion tsv
            INNER JOIN SungVersion sv ON sv.SungVersionId = tsv.SungVersionId
            INNER JOIN Track t ON t.TrackId = tsv.TrackId
            WHERE sv.PoemId = @PoemId
              AND sv.IsDeleted = 0
              AND t.IsDeleted = 0
            ORDER BY t.Title
            """;
        var results = await connection.QueryAsync(sql, new { PoemId = poemId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetEntityMediaAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                m.Url,
                m.ThumbnailUrl300 AS ThumbnailUrl,
                mt.Name AS MediaType,
                mrt.Name AS MediaRole,
                ma.IsPrimary
            FROM MediaAssignment ma
            INNER JOIN Media m ON m.MediaId = ma.MediaId
            INNER JOIN MediaType mt ON mt.MediaTypeId = m.MediaTypeId
            LEFT JOIN MediaRoleType mrt ON mrt.MediaRoleTypeId = ma.MediaRoleTypeId
            WHERE ma.EntityId = @EntityId
              AND m.IsDeleted = 0
            ORDER BY ma.IsPrimary DESC, m.MediaId
            """;
        var results = await connection.QueryAsync(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetEntityLinksAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                el.Url,
                lt.Name AS LinkType,
                el.Title
            FROM EntityLink el
            LEFT JOIN LinkType lt ON lt.LinkTypeId = el.LinkTypeId
            WHERE el.EntityId = @EntityId
              AND el.IsDeleted = 0
            ORDER BY el.EntityLinkId
            """;
        var results = await connection.QueryAsync(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetEntityCitationsAsync(
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
                c.Quote,
                c.PageNumber,
                c.Url,
                c.AccessedDate
            FROM Citation c
            LEFT JOIN Source s ON s.SourceId = c.SourceId
            WHERE c.EntityId = @EntityId
            ORDER BY c.CitationId
            """;
        var results = await connection.QueryAsync(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetEntityTagsAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT t.Name, t.Slug
            FROM TagAssignment ta
            INNER JOIN Tag t ON t.TagId = ta.TagId
            WHERE ta.EntityId = @EntityId
              AND t.IsDeleted = 0
            ORDER BY t.Name
            """;
        var results = await connection.QueryAsync(sql, new { EntityId = entityId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetEntityAliasesAsync(
        IDbConnection connection,
        int entityId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.AliasName,
                at.Name AS AliasType,
                l.Code AS Language,
                a.IsPrimary
            FROM Alias a
            LEFT JOIN AliasType at ON at.AliasTypeId = a.AliasTypeId
            LEFT JOIN Language l ON l.LanguageId = a.LanguageId
            WHERE a.EntityId = @EntityId
            ORDER BY a.IsPrimary DESC, a.AliasId
            """;
        var results = await connection.QueryAsync(sql, new { EntityId = entityId });
        return results.AsList();
    }
}
