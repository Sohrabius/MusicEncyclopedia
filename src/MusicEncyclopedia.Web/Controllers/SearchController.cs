using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for the search page.
/// Route: /{culture}/search
/// Spec reference: 8.1 (routes), 9.21 (search page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/search")]
public sealed class SearchController : Controller
{
    private readonly ISearchService _searchService;
    private readonly IDbConnection _db;
    private readonly ILogger<SearchController> _logger;
    private readonly ICacheService _cache;

    public SearchController(
        ISearchService searchService,
        IDbConnection db,
        ILogger<SearchController> logger,
        ICacheService cache)
    {
        _searchService = searchService;
        _db = db;
        _logger = logger;
        _cache = cache;
    }

    private async Task<List<LookupItem>> LoadLookupsAsync(string table, CancellationToken ct)
    {
        var cacheKey = CacheKeys.Lookup(table.ToLowerInvariant());
        var cached = await _cache.GetAsync<List<LookupItem>>(cacheKey, ct);
        if (cached is not null)
            return cached;

        var rows = await _db.QueryAsync<(string Slug, string Name)>(
            $"SELECT Slug, Name FROM {table} WHERE IsDeleted = 0 ORDER BY Name");
        var items = rows.Select(r => new LookupItem { Slug = r.Slug, Name = r.Name }).ToList();

        await _cache.SetAsync(cacheKey, items, CacheKeys.LookupDuration, ct);
        return items;
    }

    /// <summary>
    /// Search page with query, entity type filter, and paginated results (spec 9.21).
    /// GET /{culture}/search?q=...&type=...&page=...
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        string culture,
        string? q,
        string? type,
        string? genre,
        string? mood,
        string? instrument,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Search requested: culture={Culture}, q={Q}, type={Type}, genre={Genre}, mood={Mood}, instrument={Instrument}, page={Page}",
            culture, q, type, genre, mood, instrument, page);

        PagedResult<SearchResultDto>? results = null;

        if (!string.IsNullOrWhiteSpace(q))
        {
            var searchQuery = new SearchQuery
            {
                Q = q.Trim(),
                EntityType = string.IsNullOrWhiteSpace(type) ? null : type.Trim(),
                Genre = string.IsNullOrWhiteSpace(genre) ? null : genre.Trim(),
                Mood = string.IsNullOrWhiteSpace(mood) ? null : mood.Trim(),
                Instrument = string.IsNullOrWhiteSpace(instrument) ? null : instrument.Trim(),
                Culture = culture,
                Page = Math.Max(1, page),
                PageSize = 20
            };

            results = await _searchService.SearchAsync(searchQuery, cancellationToken);
        }

        // Run lookup queries sequentially: they share one scoped IDbConnection,
        // which SQL Server does not allow concurrent readers on (SQLite tolerated
        // it, SqlConnection does not). Results are cached, so this only matters on
        // a cold cache.
        var genres = await LoadLookupsAsync("Genre", cancellationToken);
        var moods = await LoadLookupsAsync("Mood", cancellationToken);
        var instruments = await LoadLookupsAsync("Instrument", cancellationToken);

        var viewModel = new SearchIndexViewModel
        {
            Query = q,
            EntityType = type,
            Genre = genre,
            Mood = mood,
            Instrument = instrument,
            Results = results,
            Culture = culture,
            Genres = genres,
            Moods = moods,
            Instruments = instruments
        };

        ViewData["Title"] = !string.IsNullOrWhiteSpace(q)
            ? $"Search: {q}"
            : "Search";
        ViewData["MetaDescription"] = "Search the Music Encyclopedia — albums, tracks, people, poems, and more.";
        ViewData["Robots"] = "noindex, follow"; // Search result pages should not be indexed
        ViewData["OgType"] = "website";

        return View(viewModel);
    }
}
