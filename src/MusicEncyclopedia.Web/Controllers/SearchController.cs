using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.DTOs;
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

    public SearchController(
        ISearchService searchService,
        IDbConnection db,
        ILogger<SearchController> logger)
    {
        _searchService = searchService;
        _db = db;
        _logger = logger;
    }

    private async Task<List<LookupItem>> LoadLookupsAsync(string table, CancellationToken ct)
    {
        var rows = await _db.QueryAsync<(string Slug, string Name)>(
            $"SELECT Slug, Name FROM {table} WHERE IsDeleted = 0 ORDER BY Name");
        return rows.Select(r => new LookupItem { Slug = r.Slug, Name = r.Name }).ToList();
    }

    /// <summary>
    /// Search page with query, entity type filter, and paginated results (spec 9.21).
    /// GET /{culture}/search?q=...&type=...&page=...
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 120, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
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

        var genresTask = LoadLookupsAsync("Genre", cancellationToken);
        var moodsTask = LoadLookupsAsync("Mood", cancellationToken);
        var instrumentsTask = LoadLookupsAsync("Instrument", cancellationToken);
        await Task.WhenAll(genresTask, moodsTask, instrumentsTask);

        var viewModel = new SearchIndexViewModel
        {
            Query = q,
            EntityType = type,
            Genre = genre,
            Mood = mood,
            Instrument = instrument,
            Results = results,
            Culture = culture,
            Genres = await genresTask,
            Moods = await moodsTask,
            Instruments = await instrumentsTask
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
