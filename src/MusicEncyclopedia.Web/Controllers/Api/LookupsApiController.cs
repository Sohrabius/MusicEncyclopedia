using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Api;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for genre, mood, and instrument lookups.
/// Spec 11.1 — Genres, Moods, Instruments endpoints.
/// These entities use Dapper directly since no dedicated service exists.
/// </summary>
public sealed class LookupsApiController : BaseApiController
{
    private readonly ICacheService _cache;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LookupsApiController> _logger;

    public LookupsApiController(
        ICacheService cache,
        IConfiguration configuration,
        ILogger<LookupsApiController> logger)
    {
        _cache = cache;
        _configuration = configuration;
        _logger = logger;
    }

    // ──────────────────────────────────────────────
    //  Genres
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/genres — list all genres.
    /// </summary>
    [HttpGet("genres")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetGenres(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 200)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 200.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Genre WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT GenreId, Slug, Name, Description, ParentGenreId
            FROM Genre
            WHERE IsDeleted = 0
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/genres/{slug} — genre detail.
    /// </summary>
    [HttpGet("genres/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetGenreBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT
                g.GenreId,
                g.Slug,
                g.Name,
                g.Description,
                pg.Name AS ParentGenreName,
                pg.Slug AS ParentGenreSlug
            FROM Genre g
            LEFT JOIN Genre pg ON pg.GenreId = g.ParentGenreId
            WHERE g.Slug = @Slug AND g.IsDeleted = 0";

        var genre = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (genre is null)
            return NotFoundResult($"Genre with slug '{slug}' not found.");

        // Load child genres
        const string childSql = @"
            SELECT GenreId, Slug, Name
            FROM Genre
            WHERE ParentGenreId = (SELECT GenreId FROM Genre WHERE Slug = @Slug) AND IsDeleted = 0
            ORDER BY Name";

        var children = (await connection.QueryAsync(childSql, new { Slug = slug })).AsList();

        return OkResult(new { genre, Children = children });
    }

    /// <summary>
    /// GET /api/v1/genres/{slug}/albums — albums in this genre.
    /// </summary>
    [HttpGet("genres/{slug}/albums")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetGenreAlbums(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var genre = await connection.QueryFirstOrDefaultAsync(
            "SELECT GenreId FROM Genre WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (genre is null)
            return NotFoundResult($"Genre with slug '{slug}' not found.");

        var countSql = "SELECT COUNT(*) FROM AlbumGenre ag INNER JOIN Album a ON a.AlbumId = ag.AlbumId WHERE ag.GenreId = @GenreId AND a.IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { GenreId = genre.GenreId });

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT a.AlbumId, a.Slug, a.Title, a.OriginalTitle, a.EnglishTitle,
                   a.CategoryName, a.ReleaseDate, a.DurationSeconds, a.CoverUrl
            FROM AlbumGenre ag
            INNER JOIN Album a ON a.AlbumId = ag.AlbumId
            WHERE ag.GenreId = @GenreId AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { GenreId = genre.GenreId, Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/genres/{slug}/tracks — tracks in this genre.
    /// </summary>
    [HttpGet("genres/{slug}/tracks")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetGenreTracks(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var genre = await connection.QueryFirstOrDefaultAsync(
            "SELECT GenreId FROM Genre WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (genre is null)
            return NotFoundResult($"Genre with slug '{slug}' not found.");

        var countSql = "SELECT COUNT(*) FROM TrackGenre tg INNER JOIN Track t ON t.TrackId = tg.TrackId WHERE tg.GenreId = @GenreId AND t.IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { GenreId = genre.GenreId });

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT t.TrackId, t.Slug, t.Title, t.OriginalTitle, t.EnglishTitle,
                   t.DurationSeconds, t.IsInstrumental, t.IsExplicit
            FROM TrackGenre tg
            INNER JOIN Track t ON t.TrackId = tg.TrackId
            WHERE tg.GenreId = @GenreId AND t.IsDeleted = 0
            ORDER BY t.Title
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { GenreId = genre.GenreId, Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    // ──────────────────────────────────────────────
    //  Moods
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/moods — list all moods.
    /// </summary>
    [HttpGet("moods")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetMoods(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 200)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 200.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Mood WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT MoodId, Slug, Name, Description
            FROM Mood
            WHERE IsDeleted = 0
            ORDER BY Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/moods/{slug} — mood detail.
    /// </summary>
    [HttpGet("moods/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetMoodBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT MoodId, Slug, Name, Description
            FROM Mood
            WHERE Slug = @Slug AND IsDeleted = 0";

        var mood = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (mood is null)
            return NotFoundResult($"Mood with slug '{slug}' not found.");

        return OkResult(mood);
    }

    /// <summary>
    /// GET /api/v1/moods/{slug}/albums — albums with this mood.
    /// </summary>
    [HttpGet("moods/{slug}/albums")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetMoodAlbums(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var mood = await connection.QueryFirstOrDefaultAsync(
            "SELECT MoodId FROM Mood WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (mood is null)
            return NotFoundResult($"Mood with slug '{slug}' not found.");

        var countSql = "SELECT COUNT(*) FROM AlbumMood am INNER JOIN Album a ON a.AlbumId = am.AlbumId WHERE am.MoodId = @MoodId AND a.IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { MoodId = mood.MoodId });

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT a.AlbumId, a.Slug, a.Title, a.OriginalTitle, a.EnglishTitle,
                   a.CategoryName, a.ReleaseDate, a.DurationSeconds, a.CoverUrl
            FROM AlbumMood am
            INNER JOIN Album a ON a.AlbumId = am.AlbumId
            WHERE am.MoodId = @MoodId AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { MoodId = mood.MoodId, Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/moods/{slug}/tracks — tracks with this mood.
    /// </summary>
    [HttpGet("moods/{slug}/tracks")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetMoodTracks(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var mood = await connection.QueryFirstOrDefaultAsync(
            "SELECT MoodId FROM Mood WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (mood is null)
            return NotFoundResult($"Mood with slug '{slug}' not found.");

        var countSql = "SELECT COUNT(*) FROM TrackMood tm INNER JOIN Track t ON t.TrackId = tm.TrackId WHERE tm.MoodId = @MoodId AND t.IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { MoodId = mood.MoodId });

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT t.TrackId, t.Slug, t.Title, t.OriginalTitle, t.EnglishTitle,
                   t.DurationSeconds, t.IsInstrumental, t.IsExplicit
            FROM TrackMood tm
            INNER JOIN Track t ON t.TrackId = tm.TrackId
            WHERE tm.MoodId = @MoodId AND t.IsDeleted = 0
            ORDER BY t.Title
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { MoodId = mood.MoodId, Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    // ──────────────────────────────────────────────
    //  Instruments
    // ──────────────────────────────────────────────

    /// <summary>
    /// GET /api/v1/instruments — list all instruments.
    /// </summary>
    [HttpGet("instruments")]
    [ResponseCache(Duration = 300)]
    public async Task<IActionResult> GetInstruments(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 200)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 200.");

        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var countSql = "SELECT COUNT(*) FROM Instrument WHERE IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql);

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT i.InstrumentId, i.Slug, i.Name, i.Description,
                   i.FamilyName, i.CountryOfOrigin, i.HistoricalNotes
            FROM Instrument i
            WHERE i.IsDeleted = 0
            ORDER BY i.Name
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }

    /// <summary>
    /// GET /api/v1/instruments/{slug} — instrument detail.
    /// </summary>
    [HttpGet("instruments/{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetInstrumentBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        const string sql = @"
            SELECT i.InstrumentId, i.Slug, i.Name, i.Description,
                   i.FamilyName, i.CountryOfOrigin, i.HistoricalNotes
            FROM Instrument i
            WHERE i.Slug = @Slug AND i.IsDeleted = 0";

        var instrument = await connection.QueryFirstOrDefaultAsync(sql, new { Slug = slug });
        if (instrument is null)
            return NotFoundResult($"Instrument with slug '{slug}' not found.");

        return OkResult(instrument);
    }

    /// <summary>
    /// GET /api/v1/instruments/{slug}/musicians — musicians who play this instrument.
    /// </summary>
    [HttpGet("instruments/{slug}/musicians")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetInstrumentMusicians(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var instrument = await connection.QueryFirstOrDefaultAsync(
            "SELECT InstrumentId FROM Instrument WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (instrument is null)
            return NotFoundResult($"Instrument with slug '{slug}' not found.");

        const string sql = @"
            SELECT DISTINCT
                p.PersonId,
                p.FullName,
                p.Slug,
                mi.Notes
            FROM MusicianInstrument mi
            INNER JOIN Person p ON p.PersonId = mi.PersonId
            WHERE mi.InstrumentId = @InstrumentId AND p.IsDeleted = 0
            ORDER BY p.FullName";

        var musicians = (await connection.QueryAsync(sql, new { InstrumentId = instrument.InstrumentId })).AsList();
        return OkListResult(musicians);
    }

    /// <summary>
    /// GET /api/v1/instruments/{slug}/tracks — tracks featuring this instrument.
    /// </summary>
    [HttpGet("instruments/{slug}/tracks")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> GetInstrumentTracks(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var instrument = await connection.QueryFirstOrDefaultAsync(
            "SELECT InstrumentId FROM Instrument WHERE Slug = @Slug AND IsDeleted = 0", new { Slug = slug });

        if (instrument is null)
            return NotFoundResult($"Instrument with slug '{slug}' not found.");

        var countSql = @"
            SELECT COUNT(DISTINCT t.TrackId)
            FROM TrackInstrument ti
            INNER JOIN Track t ON t.TrackId = ti.TrackId
            WHERE ti.InstrumentId = @InstrumentId AND t.IsDeleted = 0";
        var totalItems = await connection.ExecuteScalarAsync<int>(countSql, new { InstrumentId = instrument.InstrumentId });

        var offset = (page - 1) * pageSize;
        var dataSql = @"
            SELECT DISTINCT t.TrackId, t.Slug, t.Title, t.OriginalTitle, t.EnglishTitle,
                   t.DurationSeconds, t.IsInstrumental, t.IsExplicit
            FROM TrackInstrument ti
            INNER JOIN Track t ON t.TrackId = ti.TrackId
            WHERE ti.InstrumentId = @InstrumentId AND t.IsDeleted = 0
            ORDER BY t.Title
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = (await connection.QueryAsync(dataSql, new { InstrumentId = instrument.InstrumentId, Offset = offset, PageSize = pageSize })).AsList();
        var totalPages = totalItems > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0;

        return OkListResult(items, page, pageSize, totalItems, totalPages);
    }
}
