using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for album resources.
/// Spec 11.1 — Albums endpoints.
/// </summary>
public sealed class AlbumsApiController : BaseApiController
{
    private readonly IAlbumQueryService _albumQueryService;
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AlbumsApiController> _logger;

    public AlbumsApiController(
        IAlbumQueryService albumQueryService,
        ICacheService cache,
        IConfiguration configuration,
        ILogger<AlbumsApiController> logger)
    {
        _albumQueryService = albumQueryService;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/albums — paginated album listing.
    /// Supports ?page, ?pageSize, ?q, ?genre, ?mood.
    /// </summary>
    [HttpGet("albums")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize", "q", "genre", "mood"])]
    public async Task<IActionResult> GetAlbums(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] string? q = null,
        [FromQuery] string? genre = null,
        [FromQuery] string? mood = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        var result = await _albumQueryService.GetAlbumsAsync(
            culture: "en",
            page: page,
            pageSize: pageSize,
            genre: genre,
            mood: mood,
            q: q,
            cancellationToken: cancellationToken);

        return OkListResult(result);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug} — album detail.
    /// </summary>
    [HttpGet("albums/{slug}")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(
            slug, "en", cancellationToken);

        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkResult(album);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/tracks — track list for an album.
    /// </summary>
    [HttpGet("albums/{slug}/tracks")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumTracks(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        var tracks = await _albumQueryService.GetAlbumTracksAsync(album.AlbumId, cancellationToken);
        return OkListResult(tracks);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/credits — credits for an album.
    /// </summary>
    [HttpGet("albums/{slug}/credits")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumCredits(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkListResult(album.Credits);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/media — media attached to an album.
    /// </summary>
    [HttpGet("albums/{slug}/media")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumMedia(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkListResult(album.Media);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/links — external links for an album.
    /// </summary>
    [HttpGet("albums/{slug}/links")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumLinks(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkListResult(album.Links);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/awards — awards for an album.
    /// </summary>
    [HttpGet("albums/{slug}/awards")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumAwards(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkListResult(album.Awards);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/certifications — certifications for an album.
    /// </summary>
    [HttpGet("albums/{slug}/certifications")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumCertifications(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkListResult(album.Certifications);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/charts — chart entries for an album.
    /// </summary>
    [HttpGet("albums/{slug}/charts")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumCharts(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        return OkListResult(album.ChartEntries);
    }

    /// <summary>
    /// GET /api/v1/albums/{slug}/related — related albums.
    /// Uses Dapper to query album relations since the service does not expose this directly.
    /// </summary>
    [HttpGet("albums/{slug}/related")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetAlbumRelated(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var album = await _albumQueryService.GetAlbumBySlugAsync(slug, "en", cancellationToken);
        if (album is null)
            return NotFoundResult($"Album with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                a.AlbumId,
                a.Slug,
                a.Title,
                a.ReleaseDate,
                m.Url AS CoverUrl,
                art.Name AS RelationType
            FROM AlbumRelation ar
            INNER JOIN Album a ON a.AlbumId = ar.RelatedAlbumId
            LEFT JOIN AlbumRelationType art ON art.AlbumRelationTypeId = ar.AlbumRelationTypeId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE ar.AlbumId = @AlbumId AND a.IsDeleted = 0
            UNION
            SELECT
                a.AlbumId,
                a.Slug,
                a.Title,
                a.ReleaseDate,
                m.Url AS CoverUrl,
                art.Name AS RelationType
            FROM AlbumRelation ar
            INNER JOIN Album a ON a.AlbumId = ar.AlbumId
            LEFT JOIN AlbumRelationType art ON art.AlbumRelationTypeId = ar.AlbumRelationTypeId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE ar.RelatedAlbumId = @AlbumId AND a.IsDeleted = 0";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        var related = (await connection.QueryAsync(sql, new { album.AlbumId })).AsList();

        return OkListResult(related);
    }
}
