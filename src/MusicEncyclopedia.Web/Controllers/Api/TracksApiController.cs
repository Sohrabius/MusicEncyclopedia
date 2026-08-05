using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for track resources.
/// Spec 11.1 — Tracks endpoints.
/// </summary>
public sealed class TracksApiController : BaseApiController
{
    private readonly ITrackQueryService _trackQueryService;
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TracksApiController> _logger;

    public TracksApiController(
        ITrackQueryService trackQueryService,
        ICacheService cache,
        IConfiguration configuration,
        ILogger<TracksApiController> logger)
    {
        _trackQueryService = trackQueryService;
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/tracks — paginated track listing.
    /// Supports ?page, ?pageSize, ?q, ?genre, ?mood.
    /// </summary>
    [HttpGet("tracks")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "pageSize", "q", "genre", "mood"])]
    public async Task<IActionResult> GetTracks(
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

        var result = await _trackQueryService.GetTracksAsync(
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
    /// GET /api/v1/tracks/{slug} — track detail.
    /// </summary>
    [HttpGet("tracks/{slug}")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        return OkResult(track);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/albums — album appearances for a track.
    /// Uses Dapper since TrackDetailDto already contains albums in its Albums property.
    /// </summary>
    [HttpGet("tracks/{slug}/albums")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackAlbums(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        return OkListResult(track.Albums);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/credits — credits for a track.
    /// </summary>
    [HttpGet("tracks/{slug}/credits")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackCredits(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        return OkListResult(track.Credits);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/musicians — musician credits for a track.
    /// </summary>
    [HttpGet("tracks/{slug}/musicians")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackMusicians(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        return OkListResult(track.Musicians);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/lyrics — lyrics for a track.
    /// Respects lyrics availability. Returns 404 if no lyrics or restricted.
    /// </summary>
    [HttpGet("tracks/{slug}/lyrics")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackLyrics(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        // Respect lyrics availability
        if (track.IsInstrumental)
            return NotFoundResult("This track is instrumental and has no lyrics.");

        if (string.IsNullOrWhiteSpace(track.LyricsAvailabilityName) ||
            track.LyricsAvailabilityName.Equals("None", StringComparison.OrdinalIgnoreCase))
            return NotFoundResult("No lyrics available for this track.");

        if (track.LyricsAvailabilityName.Equals("Restricted", StringComparison.OrdinalIgnoreCase) ||
            track.LyricsAvailabilityName.Equals("Request", StringComparison.OrdinalIgnoreCase))
            return NotFoundResult("Lyrics for this track are restricted.");

        // Fetch lyrics via Dapper (SungVersion text associated with this track)
        const string sql = @"
            SELECT
                sv.SungVersionId,
                sv.Title AS SungVersionTitle,
                sv.Slug AS SungVersionSlug,
                sv.Text AS LyricsText,
                sv.IsCanonical,
                vs.Name AS VocalStyleName,
                p.Title AS PoemTitle,
                p.Slug AS PoemSlug,
                pers.FullName AS PoetName,
                pers.Slug AS PoetSlug
            FROM TrackSungVersion tsv
            INNER JOIN SungVersion sv ON sv.SungVersionId = tsv.SungVersionId
            LEFT JOIN VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
            LEFT JOIN Poem p ON p.PoemId = sv.PoemId
            LEFT JOIN Person pers ON pers.PersonId = p.PersonId
            WHERE tsv.TrackId = @TrackId AND sv.IsDeleted = 0";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        var lyrics = (await connection.QueryAsync(sql, new { track.TrackId })).AsList();

        if (lyrics.Count == 0)
            return NotFoundResult("No lyrics found for this track.");

        return OkResult(lyrics);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/poems — poems associated with a track.
    /// </summary>
    [HttpGet("tracks/{slug}/poems")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackPoems(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                p.PoemId,
                p.Title,
                p.Slug,
                p.CanonicalText,
                pers.FullName AS PoetName,
                pers.Slug AS PoetSlug
            FROM TrackSungVersion tsv
            INNER JOIN SungVersion sv ON sv.SungVersionId = tsv.SungVersionId
            INNER JOIN Poem p ON p.PoemId = sv.PoemId
            LEFT JOIN Person pers ON pers.PersonId = p.PersonId
            WHERE tsv.TrackId = @TrackId AND p.IsDeleted = 0 AND sv.IsDeleted = 0
            GROUP BY p.PoemId, p.Title, p.Slug, p.CanonicalText, pers.FullName, pers.Slug";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        var poems = (await connection.QueryAsync(sql, new { track.TrackId })).AsList();

        return OkListResult(poems);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/related — related tracks.
    /// </summary>
    [HttpGet("tracks/{slug}/related")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackRelated(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                t.TrackId,
                t.Slug,
                t.Title,
                t.DurationSeconds,
                trt.Name AS RelationType
            FROM TrackRelation tr
            INNER JOIN Track t ON t.TrackId = tr.RelatedTrackId
            LEFT JOIN TrackRelationType trt ON trt.TrackRelationTypeId = tr.TrackRelationTypeId
            WHERE tr.TrackId = @TrackId AND t.IsDeleted = 0
            UNION
            SELECT
                t.TrackId,
                t.Slug,
                t.Title,
                t.DurationSeconds,
                trt.Name AS RelationType
            FROM TrackRelation tr
            INNER JOIN Track t ON t.TrackId = tr.TrackId
            LEFT JOIN TrackRelationType trt ON trt.TrackRelationTypeId = tr.TrackRelationTypeId
            WHERE tr.RelatedTrackId = @TrackId AND t.IsDeleted = 0";

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        var related = (await connection.QueryAsync(sql, new { track.TrackId })).AsList();

        return OkListResult(related);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/media — media for a track.
    /// </summary>
    [HttpGet("tracks/{slug}/media")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackMedia(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        return OkListResult(track.Media);
    }

    /// <summary>
    /// GET /api/v1/tracks/{slug}/links — external links for a track.
    /// </summary>
    [HttpGet("tracks/{slug}/links")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = ["slug"])]
    public async Task<IActionResult> GetTrackLinks(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var track = await _trackQueryService.GetTrackBySlugAsync(slug, "en", cancellationToken);
        if (track is null)
            return NotFoundResult($"Track with slug '{slug}' not found.");

        return OkListResult(track.Links);
    }
}
