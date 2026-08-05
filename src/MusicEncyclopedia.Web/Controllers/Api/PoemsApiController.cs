using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for poem and sung-version resources.
/// Spec 11.1 — Poems and Sung Versions endpoints.
/// Uses Dapper directly since no dedicated service exists.
/// </summary>
public sealed class PoemsApiController : BaseApiController
{
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PoemsApiController> _logger;

    public PoemsApiController(
        ICacheService cache,
        IConfiguration configuration,
        ILogger<PoemsApiController> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  Poems
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/poems — paginated poem listing.
    /// </summary>
    [HttpGet("poems")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize"])]
    public async Task<IActionResult> GetPoems(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Poem WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT
                p.PoemId,
                p.Slug,
                p.Title,
                p.OriginalTitle,
                p.EnglishTitle,
                pers.FullName AS PoetName,
                pers.Slug AS PoetSlug,
                pub.Title AS PublicationTitle,
                pub.Slug AS PublicationSlug
            FROM Poem p
            LEFT JOIN Person pers ON pers.PersonId = p.PersonId
            LEFT JOIN Publication pub ON pub.PublicationId = p.PublicationId
            WHERE p.IsDeleted = 0
            ORDER BY p.Title
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/poems/{slug} — poem detail.
    /// </summary>
    [HttpGet("poems/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetPoemBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                p.PoemId,
                p.EntityId,
                p.Slug,
                p.Title,
                p.OriginalTitle,
                p.EnglishTitle,
                p.CanonicalText,
                p.Notes,
                p.Copyright,
                p.ExternalReferenceUrl,
                p.OriginalPublicationDate,
                pers.PersonId AS PoetPersonId,
                pers.FullName AS PoetName,
                pers.Slug AS PoetSlug,
                pub.PublicationId,
                pub.Title AS PublicationTitle,
                pub.Slug AS PublicationSlug
            FROM Poem p
            LEFT JOIN Person pers ON pers.PersonId = p.PersonId
            LEFT JOIN Publication pub ON pub.PublicationId = p.PublicationId
            WHERE p.Slug = @Slug AND p.IsDeleted = 0";

        var poem = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (poem is null)
            return NotFoundResult($"Poem with slug '{slug}' not found.");

        return OkResult(poem);
    }

    /// <summary>
    /// GET /api/v1/poems/{slug}/sung-versions — sung versions of a poem.
    /// </summary>
    [HttpGet("poems/{slug}/sung-versions")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetPoemSungVersions(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var poem = await connection.QueryFirstOrDefaultAsync(
            "SELECT PoemId FROM Poem WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (poem is null)
            return NotFoundResult($"Poem with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                sv.SungVersionId,
                sv.Slug,
                sv.Title,
                sv.Text,
                sv.IsCanonical,
                sv.Notes,
                vs.Name AS VocalStyleName
            FROM SungVersion sv
            LEFT JOIN VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
            WHERE sv.PoemId = @PoemId AND sv.IsDeleted = 0
            ORDER BY sv.IsCanonical DESC, sv.Title";

        var sungVersions = (await connection.QueryAsync(sql, new { PoemId = poem.PoemId })).AsList();
        return OkListResult(sungVersions);
    }

    /// <summary>
    /// GET /api/v1/poems/{slug}/tracks — tracks using a poem (via sung versions).
    /// </summary>
    [HttpGet("poems/{slug}/tracks")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetPoemTracks(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var poem = await connection.QueryFirstOrDefaultAsync(
            "SELECT PoemId FROM Poem WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (poem is null)
            return NotFoundResult($"Poem with slug '{slug}' not found.");

        const string sql = @"
            SELECT DISTINCT
                t.TrackId,
                t.Slug,
                t.Title,
                t.DurationSeconds,
                t.IsInstrumental,
                t.IsExplicit,
                sv.Title AS SungVersionTitle,
                sv.Slug AS SungVersionSlug
            FROM TrackSungVersion tsv
            INNER JOIN SungVersion sv ON sv.SungVersionId = tsv.SungVersionId
            INNER JOIN Track t ON t.TrackId = tsv.TrackId
            WHERE sv.PoemId = @PoemId AND t.IsDeleted = 0 AND sv.IsDeleted = 0
            ORDER BY t.Title";

        var tracks = (await connection.QueryAsync(sql, new { PoemId = poem.PoemId })).AsList();
        return OkListResult(tracks);
    }

    // ──────────────────────────────────────────────
    //  Sung Versions
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/sung-versions/{slug} — sung version detail.
    /// </summary>
    [HttpGet("sung-versions/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetSungVersionBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                sv.SungVersionId,
                sv.EntityId,
                sv.Slug,
                sv.Title,
                sv.Text,
                sv.Notes,
                sv.IsCanonical,
                vs.Name AS VocalStyleName,
                p.PoemId,
                p.Title AS PoemTitle,
                p.Slug AS PoemSlug,
                p.CanonicalText AS PoemCanonicalText,
                pers.FullName AS PoetName,
                pers.Slug AS PoetSlug
            FROM SungVersion sv
            LEFT JOIN VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
            LEFT JOIN Poem p ON p.PoemId = sv.PoemId
            LEFT JOIN Person pers ON pers.PersonId = p.PersonId
            WHERE sv.Slug = @Slug AND sv.IsDeleted = 0";

        var sungVersion = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (sungVersion is null)
            return NotFoundResult($"Sung version with slug '{slug}' not found.");

        return OkResult(sungVersion);
    }

    /// <summary>
    /// GET /api/v1/sung-versions/{slug}/tracks — tracks using this sung version.
    /// </summary>
    [HttpGet("sung-versions/{slug}/tracks")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetSungVersionTracks(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var sungVersion = await connection.QueryFirstOrDefaultAsync(
            "SELECT SungVersionId FROM SungVersion WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (sungVersion is null)
            return NotFoundResult($"Sung version with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                t.TrackId,
                t.Slug,
                t.Title,
                t.DurationSeconds,
                t.IsInstrumental,
                t.IsExplicit
            FROM TrackSungVersion tsv
            INNER JOIN Track t ON t.TrackId = tsv.TrackId
            WHERE tsv.SungVersionId = @SungVersionId AND t.IsDeleted = 0
            ORDER BY t.Title";

        var tracks = (await connection.QueryAsync(sql, new { SungVersionId = sungVersion.SungVersionId })).AsList();
        return OkListResult(tracks);
    }
}
