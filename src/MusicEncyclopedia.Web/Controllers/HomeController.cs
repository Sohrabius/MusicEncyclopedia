using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Services.Infrastructure;
using MusicEncyclopedia.Web.ViewModels;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Home controller handling the main public landing page and error pages.
/// Route is culture-aware per the application's localization strategy.
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}")]
public sealed class HomeController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<HomeController> _logger;
    private readonly ICacheService _cache;

    public HomeController(IDbConnection db, ILogger<HomeController> logger, ICacheService cache)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    /// Displays the home page with hero section, featured albums,
    /// latest additions, and browse-by links.
    /// Route: /{culture}
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Home")]
    [Route("Home/Index")]
    public async Task<IActionResult> Index(string culture, CancellationToken cancellationToken = default)
    {
        // Spec §15.1 home cache key: home:{culture}. Only successful loads are cached.
        var cacheKey = CacheKeys.Home(culture);
        var cached = await _cache.GetAsync<HomeViewModel>(cacheKey, cancellationToken);
        if (cached is not null)
            return View(cached);

        var viewModel = await LoadHomeViewModelAsync(culture, cancellationToken);

        await _cache.SetAsync(cacheKey, viewModel, CacheKeys.HomeDuration, cancellationToken);
        return View(viewModel);
    }

    private async Task<HomeViewModel> LoadHomeViewModelAsync(
        string culture,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Home page requested for culture: {Culture}", culture);

        var viewModel = new HomeViewModel
        {
            CurrentCulture = culture,
            MetaDescription = "Explore the complete encyclopedia of music — albums, tracks, artists, lyrics, and more."
        };

        try
        {
            var isSqlite = SqlDialect.IsSqliteConnection(_db);
            var top8 = SqlDialect.Pagination(isSqlite, "0", "8");
            var top10 = SqlDialect.Pagination(isSqlite, "0", "10");
            var top6 = SqlDialect.Pagination(isSqlite, "0", "6");

            // ── Featured albums: most recently added, with the primary artist.
            // The artist is aggregated with MIN() (instead of a correlated subquery)
            // so the query works on both SQLite and SQL Server.
            var featuredSql = $"""
                SELECT
                    a.Slug,
                    a.Title,
                    m.Url AS CoverUrl,
                    a.ReleaseDate,
                    art.Artist
                FROM Album a
                LEFT JOIN Media m ON m.MediaId = a.CoverMediaId AND m.IsDeleted = 0
                LEFT JOIN (
                    SELECT c.EntityId, MIN(p.FullName) AS Artist
                    FROM Credit c
                    INNER JOIN Person p ON p.PersonId = c.PersonId
                    WHERE c.IsPrimary = 1 AND p.IsDeleted = 0
                    GROUP BY c.EntityId
                ) art ON art.EntityId = a.EntityId
                WHERE a.IsDeleted = 0
                ORDER BY a.CreatedAt DESC
                {top8}
                """;

            var featuredRows = await _db.QueryAsync<(string Slug, string Title, string? CoverUrl, DateTime? ReleaseDate, string? Artist)>(
                featuredSql);

            viewModel.FeaturedAlbums = featuredRows
                .Select(r => new FeaturedAlbumItem
                {
                    Slug = r.Slug,
                    Title = r.Title,
                    CoverUrl = r.CoverUrl,
                    Artist = r.Artist,
                    Year = r.ReleaseDate?.Year
                })
                .ToList();

            // ── Latest albums: newest additions.
            var latestSql = $"""
                SELECT
                    a.Slug,
                    a.Title,
                    m.Url AS CoverUrl,
                    a.ReleaseDate,
                    art.Artist
                FROM Album a
                LEFT JOIN Media m ON m.MediaId = a.CoverMediaId AND m.IsDeleted = 0
                LEFT JOIN (
                    SELECT c.EntityId, MIN(p.FullName) AS Artist
                    FROM Credit c
                    INNER JOIN Person p ON p.PersonId = c.PersonId
                    WHERE c.IsPrimary = 1 AND p.IsDeleted = 0
                    GROUP BY c.EntityId
                ) art ON art.EntityId = a.EntityId
                WHERE a.IsDeleted = 0
                ORDER BY a.CreatedAt DESC
                {top8}
                """;

            var latestRows = await _db.QueryAsync<(string Slug, string Title, string? CoverUrl, DateTime? ReleaseDate, string? Artist)>(
                latestSql);

            viewModel.LatestAlbums = latestRows
                .Select(r => new FeaturedAlbumItem
                {
                    Slug = r.Slug,
                    Title = r.Title,
                    CoverUrl = r.CoverUrl,
                    Artist = r.Artist,
                    Year = r.ReleaseDate?.Year
                })
                .ToList();

            // ── Essential tracks: recent tracks with their primary artist.
            var tracksSql = $"""
                SELECT
                    t.Slug,
                    t.Title,
                    t.DurationSeconds,
                    art.Artist
                FROM Track t
                LEFT JOIN (
                    SELECT c.EntityId, MIN(p.FullName) AS Artist
                    FROM Credit c
                    INNER JOIN Person p ON p.PersonId = c.PersonId
                    WHERE p.IsDeleted = 0
                    GROUP BY c.EntityId
                ) art ON art.EntityId = t.EntityId
                WHERE t.IsDeleted = 0
                ORDER BY t.CreatedAt DESC
                {top10}
                """;

            var trackRows = await _db.QueryAsync<(string Slug, string Title, int? DurationSeconds, string? Artist)>(
                tracksSql);

            viewModel.EssentialTracks = trackRows
                .Select(r => new FeaturedTrackItem
                {
                    Slug = r.Slug,
                    Title = r.Title,
                    Artist = r.Artist,
                    Duration = FormatDuration(r.DurationSeconds)
                })
                .ToList();

            // ── Featured poems: recent poems with their poet.
            var poemsSql = $"""
                SELECT
                    po.Slug,
                    po.Title,
                    pers.FullName AS Poet
                FROM Poem po
                LEFT JOIN Person pers ON pers.PersonId = po.PersonId AND pers.IsDeleted = 0
                WHERE po.IsDeleted = 0
                ORDER BY po.CreatedAt DESC
                {top6}
                """;

            var poemRows = await _db.QueryAsync<(string Slug, string Title, string? Poet)>(poemsSql);

            viewModel.FeaturedPoems = poemRows
                .Select(r => new FeaturedPoemItem
                {
                    Slug = r.Slug,
                    Title = r.Title,
                    Poet = r.Poet
                })
                .ToList();

            // ── Browse-by links: top genres, moods, and instruments.
            var genreRows = await _db.QueryAsync<(string Slug, string Name)>(
                $"SELECT Slug, Name FROM Genre WHERE IsDeleted = 0 ORDER BY Name {top8}");
            viewModel.Genres = genreRows.Select(r => new BrowseLink { Slug = r.Slug, Name = r.Name }).ToList();

            var moodRows = await _db.QueryAsync<(string Slug, string Name)>(
                $"SELECT Slug, Name FROM Mood WHERE IsDeleted = 0 ORDER BY Name {top8}");
            viewModel.Moods = moodRows.Select(r => new BrowseLink { Slug = r.Slug, Name = r.Name }).ToList();

            var instrumentRows = await _db.QueryAsync<(string Slug, string Name)>(
                $"SELECT Slug, Name FROM Instrument WHERE IsDeleted = 0 ORDER BY Name {top8}");
            viewModel.Instruments = instrumentRows.Select(r => new BrowseLink { Slug = r.Slug, Name = r.Name }).ToList();
        }
        catch (Exception ex)
        {
            // The home page must never fail — fall back to an empty model.
            _logger.LogError(ex, "Failed to load home page content for culture={Culture}", culture);
        }

        return viewModel;
    }

    /// <summary>
    /// Generic error page for unhandled exceptions.
    /// Route: /{culture}/Home/Error
    /// </summary>
    [HttpGet]
    [Route("Home/Error")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Error()
    {
        var requestId = System.Diagnostics.Activity.Current?.Id
                        ?? HttpContext.TraceIdentifier;

        return View(new ErrorViewModel
        {
            RequestId = requestId,
            ShowRequestId = !string.IsNullOrEmpty(requestId)
        });
    }

    private static string FormatDuration(int? seconds)
    {
        if (!seconds.HasValue || seconds <= 0)
            return "";

        var minutes = seconds.Value / 60;
        var remainder = seconds.Value % 60;
        return $"{minutes}:{remainder:00}";
    }
}
