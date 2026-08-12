using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for sung version detail pages.
/// Route: /{culture}/sung-versions/{slug}
/// Spec references: 8.1 (routes), 9.13 (detail page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/sung-versions")]
public sealed class SungVersionsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<SungVersionsController> _logger;

    public SungVersionsController(
        IDbConnection db,
        ILogger<SungVersionsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Sung version detail page per spec 9.13.
    /// Displays sung text, original poem link, poet, vocal style, tracks using it.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("SungVersion detail requested: culture={Culture}, slug={Slug}", culture, slug);

        try
        {
            // Load sung version
            var svSql = """
                SELECT
                    sv.SungVersionId,
                    sv.EntityId,
                    sv.Slug,
                    sv.Title,
                    sv.Text,
                    sv.Notes,
                    sv.IsCanonical,
                    sv.PoemId,
                    vs.Name AS VocalStyleName,
                    p.Slug AS PoemSlug,
                    p.Title AS PoemTitle,
                    p.CanonicalText AS PoemCanonicalText,
                    pers.FullName AS PoetName,
                    pers.Slug AS PoetSlug
                FROM SungVersion sv
                LEFT JOIN VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
                INNER JOIN Poem p ON p.PoemId = sv.PoemId AND p.IsDeleted = 0
                LEFT JOIN Person pers ON pers.PersonId = p.PersonId AND pers.IsDeleted = 0
                WHERE sv.Slug = @Slug AND sv.IsDeleted = 0
                """;

            var sv = await _db.QuerySingleOrDefaultAsync(svSql, new { Slug = slug });

            if (sv is null)
            {
                _logger.LogWarning("SungVersion not found: slug={Slug}, culture={Culture}", slug, culture);
                return NotFound();
            }

            var entityId = (int)((dynamic)sv).EntityId;
            var sungVersionId = (int)((dynamic)sv).SungVersionId;

            // Load related data sequentially (safe for both SQL Server and SQLite)
            var tracks = await GetTracksForSungVersionAsync(_db, sungVersionId, cancellationToken);
            var media = await GetEntityMediaAsync(_db, entityId, cancellationToken);
            var links = await GetEntityLinksAsync(_db, entityId, cancellationToken);
            var citations = await GetEntityCitationsAsync(_db, entityId, cancellationToken);
            var tags = await GetEntityTagsAsync(_db, entityId, cancellationToken);

            var detail = new
            {
                sv.Slug,
                sv.Title,
                sv.Text,
                sv.Notes,
                sv.IsCanonical,
                sv.VocalStyleName,
                Poem = new
                {
                    Title = (string)((dynamic)sv).PoemTitle,
                    Slug = (string)((dynamic)sv).PoemSlug,
                    CanonicalText = (string?)((dynamic)sv).PoemCanonicalText
                },
                Poet = ((string)((dynamic)sv).PoetName) is not null
                    ? new { FullName = (string)((dynamic)sv).PoetName, Slug = (string)((dynamic)sv).PoetSlug }
                    : null,
                Tracks = tracks,
                Media = media,
                Links = links,
                Citations = citations,
                Tags = tags
            };

            var viewModel = new SungVersionDetailViewModel
            {
                SungVersion = detail,
                Culture = culture
            };

            ViewData["Title"] = ((dynamic)sv).Title ?? "Sung Version";
            ViewData["MetaDescription"] = $"Sung version: {((dynamic)sv).Title}";
            ViewData["Robots"] = "index, follow";
            ViewData["OgType"] = "music.song";
            ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/sung-versions/{slug}";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load sung version detail: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────────

    private static async Task<IReadOnlyList<dynamic>> GetTracksForSungVersionAsync(
        IDbConnection connection,
        int sungVersionId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                t.Slug,
                t.Title,
                t.DurationSeconds
            FROM TrackSungVersion tsv
            INNER JOIN Track t ON t.TrackId = tsv.TrackId
            WHERE tsv.SungVersionId = @SungVersionId
              AND t.IsDeleted = 0
            ORDER BY t.Title
            """;
        var results = await connection.QueryAsync(sql, new { SungVersionId = sungVersionId });
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
}
