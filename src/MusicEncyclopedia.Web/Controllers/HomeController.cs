using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Services.Infrastructure;
using MusicEncyclopedia.Web.Constants;
using MusicEncyclopedia.Web.ViewModels;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Home controller handling the main public landing page and error pages.
/// Route is culture-aware per the application's localization strategy.
///
/// Home page layout (HOME_REDESIGN_PLAN.md):
///   banner (search-first) → album catalog (category chips + lazy-load grid
///   → numbered pagination past 100) with a sidebar (random poem + about card).
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}")]
public sealed class HomeController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<HomeController> _logger;
    private readonly ICacheService _cache;
    private readonly IAlbumQueryService _albums;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public HomeController(
        IDbConnection db,
        ILogger<HomeController> logger,
        ICacheService cache,
        IAlbumQueryService albums,
        IStringLocalizer<SharedResources> localizer)
    {
        _db = db;
        _logger = logger;
        _cache = cache;
        _albums = albums;
        _localizer = localizer;
    }

    /// <summary>
    /// Root redirect: forwards "/" (and bare /Home /Home/Index) to the
    /// default-culture home page so visitors never need to type "/fa" in the
    /// address bar. The culture middleware and route constraints keep every
    /// other page culture-prefixed; this is the only exception.
    /// </summary>
    [HttpGet]
    [Route("~/")]
    [Route("~/Home")]
    [Route("~/Home/Index")]
    public IActionResult Root()
    {
        // Redirect to the clean /{culture} URL — Index's attribute route "" under
        // the class prefix makes /fa resolve to the same home page.
        return Redirect($"/{CultureConstants.DefaultCulture}");
    }

    /// <summary>
    /// Displays the album-first home page: banner, category chips, first page of
    /// albums (20), a random-poem sidebar card and an about card.
    /// Route: /{culture}  (with optional ?page= and ?category=)
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Home")]
    [Route("Home/Index")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Home page requested for culture={Culture}, page={Page}, category={Category}",
            culture, page, category);

        var viewModel = await LoadHomeViewModelAsync(culture, page, category, cancellationToken);
        return View(viewModel);
    }

    /// <summary>
    /// Partial endpoint for the lazy-loaded album grid ("Load more").
    /// Returns only the album cards for the requested page + category.
    /// Route: /{culture}/home/albums?page=N&amp;category=C
    /// </summary>
    [HttpGet]
    [Route("home/albums")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = ["page", "category"])]
    public async Task<IActionResult> AlbumGrid(
        string culture,
        int page = 1,
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);

        var result = await _albums.GetAlbumsAsync(
            culture,
            page,
            HomeConstants.InitialPageSize,
            category: category,
            cancellationToken: cancellationToken);

        return PartialView("_AlbumCardGrid", result);
    }

    /// <summary>
    /// Returns the random-poem sidebar card as HTML (used by the "show another
    /// poem" shuffle button). Route: /{culture}/home/random-poem
    /// </summary>
    [HttpGet]
    [Route("home/random-poem")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> RandomPoemCard(
        string culture,
        CancellationToken cancellationToken = default)
    {
        var poem = await LoadRandomPoemAsync(cancellationToken);
        return PartialView("_RandomPoemCard", poem);
    }

    private async Task<HomeViewModel> LoadHomeViewModelAsync(
        string culture,
        int page,
        string? category,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Loading home page content for culture: {Culture}", culture);

        var viewModel = new HomeViewModel
        {
            CurrentCulture = culture,
            SelectedCategory = category,
            MetaDescription = _localizer["Home.MetaDescription"]
        };

        try
        {
            var top8 = SqlDialect.Pagination("0", "8");

            // ── Banner hint chips: genres / moods / instruments (cached 1h).
            var browse = await GetCachedAsync("home:browse", cancellationToken, async () =>
            {
                var genreRows = await _db.QueryAsync<(string Slug, string Name)>(
                    $"SELECT Slug, Name FROM Genre WHERE IsDeleted = 0 ORDER BY Name {top8}");
                var moodRows = await _db.QueryAsync<(string Slug, string Name)>(
                    $"SELECT Slug, Name FROM Mood WHERE IsDeleted = 0 ORDER BY Name {top8}");
                var instrumentRows = await _db.QueryAsync<(string Slug, string Name)>(
                    $"SELECT Slug, Name FROM Instrument WHERE IsDeleted = 0 ORDER BY Name {top8}");

                return new HomeBrowseData
                {
                    Genres = genreRows.Select(r => new BrowseLink { Slug = r.Slug, Name = r.Name }).ToList(),
                    Moods = moodRows.Select(r => new BrowseLink { Slug = r.Slug, Name = r.Name }).ToList(),
                    Instruments = instrumentRows.Select(r => new BrowseLink { Slug = r.Slug, Name = r.Name }).ToList()
                };
            });

            viewModel.Genres = browse.Genres;
            viewModel.Moods = browse.Moods;
            viewModel.Instruments = browse.Instruments;

            // ── Category filter chips with album counts (cached 1h).
            viewModel.Categories = await GetCachedAsync("home:categories", cancellationToken, async () =>
            {
                const string sql = """
                    SELECT ac.Code, ac.Name, COUNT(a.AlbumId) AS AlbumCount
                    FROM AlbumCategory ac
                    LEFT JOIN Album a ON a.AlbumCategoryId = ac.AlbumCategoryId AND a.IsDeleted = 0
                    GROUP BY ac.Code, ac.Name
                    ORDER BY ac.Name
                    """;
                var rows = await _db.QueryAsync<AlbumCategoryChip>(sql);
                return rows.AsList();
            });

            // ── About card stats (cached 1h).
            viewModel.About = await GetCachedAsync("home:about", cancellationToken, async () =>
            {
                const string sql = """
                    SELECT
                        (SELECT COUNT(1) FROM Album WHERE IsDeleted = 0)  AS Albums,
                        (SELECT COUNT(1) FROM Track WHERE IsDeleted = 0)  AS Tracks,
                        (SELECT COUNT(1) FROM Person WHERE IsDeleted = 0) AS People
                    """;
                var row = await _db.QuerySingleAsync<AboutStats>(sql);
                return row;
            });

            // ── Album catalog: first page (20) — service caches per page/category.
            var result = await _albums.GetAlbumsAsync(
                culture,
                page,
                HomeConstants.InitialPageSize,
                category: category,
                cancellationToken: cancellationToken);

            viewModel.Albums = result;
            viewModel.LoadedCount = Math.Min(page * HomeConstants.InitialPageSize, result.TotalItems);

            // ── Random poem: resolved per request, NEVER cached.
            viewModel.RandomPoem = await LoadRandomPoemAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // The home page must never fail — fall back to an empty model.
            _logger.LogError(ex, "Failed to load home page content for culture={Culture}", culture);
        }

        return viewModel;
    }

    /// <summary>
    /// Picks a random poem that has at least one sung-version track (and prefers
    /// tracks that appear on an album), then falls back to a plain random poem.
    /// Never cached — must change on every page load.
    /// </summary>
    private async Task<RandomPoemCard?> LoadRandomPoemAsync(CancellationToken cancellationToken)
    {
        try
        {
            var random = SqlDialect.RandomOrder();
            var limit = SqlDialect.Pagination("0", "1");

            // Prefer rows where the track belongs to an album.
            var poemSql = $"""
                SELECT
                    p.Slug        AS PoemSlug,
                    p.Title       AS PoemTitle,
                    pers.FullName AS Poet,
                    t.Slug        AS TrackSlug,
                    t.Title       AS TrackTitle,
                    a.Slug        AS AlbumSlug,
                    a.Title       AS AlbumTitle
                FROM Poem p
                LEFT JOIN Person pers ON pers.PersonId = p.PersonId AND pers.IsDeleted = 0
                INNER JOIN SungVersion sv ON sv.PoemId = p.PoemId AND sv.IsDeleted = 0
                INNER JOIN TrackSungVersion tsv ON tsv.SungVersionId = sv.SungVersionId
                INNER JOIN Track t ON t.TrackId = tsv.TrackId AND t.IsDeleted = 0
                LEFT JOIN AlbumTrack at ON at.TrackId = t.TrackId
                LEFT JOIN Album a ON a.AlbumId = at.AlbumId AND a.IsDeleted = 0
                WHERE p.IsDeleted = 0
                ORDER BY CASE WHEN a.AlbumId IS NULL THEN 1 ELSE 0 END, {random}
                {limit}
                """;

            var poem = await _db.QuerySingleOrDefaultAsync<RandomPoemCard>(poemSql);

            if (poem is not null)
                return poem;

            // Fallback: any poem, without track/album links.
            var fallbackSql = $"""
                SELECT
                    p.Slug        AS PoemSlug,
                    p.Title       AS PoemTitle,
                    pers.FullName AS Poet
                FROM Poem p
                LEFT JOIN Person pers ON pers.PersonId = p.PersonId AND pers.IsDeleted = 0
                WHERE p.IsDeleted = 0
                ORDER BY {random}
                {limit}
                """;

            return await _db.QuerySingleOrDefaultAsync<RandomPoemCard>(fallbackSql);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load random poem for home sidebar");
            return null;
        }
    }

    /// <summary>
    /// Reads a deterministic home fragment from cache or loads + caches it
    /// (lookup duration — 1h — so it stays fresh enough after admin edits).
    /// </summary>
    private async Task<T> GetCachedAsync<T>(
        string name,
        CancellationToken cancellationToken,
        Func<Task<T>> loader) where T : class
    {
        var key = CacheKeys.Lookup(name);
        var cached = await _cache.GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var value = await loader();
        await _cache.SetAsync(key, value, CacheKeys.LookupDuration, cancellationToken);
        return value;
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
}
