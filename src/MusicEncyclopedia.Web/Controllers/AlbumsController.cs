using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for album listing and detail pages.
/// Routes: /{culture}/albums and /{culture}/albums/{slug}
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/albums")]
public sealed class AlbumsController : Controller
{
    private readonly IAlbumQueryService _albumQueryService;
    private readonly ILogger<AlbumsController> _logger;

    public AlbumsController(
        IAlbumQueryService albumQueryService,
        ILogger<AlbumsController> logger)
    {
        _albumQueryService = albumQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Album listing page (9.2).
    /// Supports filtering by genre, mood, year, and search text.
    /// Default sort: releaseDate DESC.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        string? genre = null,
        string? mood = null,
        int? year = null,
        string? q = null,
        string sort = "releaseDate",
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Album list requested: culture={Culture}, page={Page}, genre={Genre}, mood={Mood}, year={Year}, q={Q}, sort={Sort}",
            culture, page, genre, mood, year, q, sort);

        // Flatten year into a search term if provided
        var searchQuery = !string.IsNullOrWhiteSpace(q) ? q : year?.ToString();

        var result = await _albumQueryService.GetAlbumsAsync(
            culture,
            page,
            pageSize: 24,
            sort: sort,
            genre: genre,
            mood: mood,
            q: searchQuery,
            cancellationToken: cancellationToken);

        var viewModel = new AlbumListViewModel
        {
            Items = result,
            Culture = culture,
            CurrentFilters = new AlbumListFilterViewModel
            {
                GenreSlug = genre,
                MoodSlug = mood,
                Year = year,
                SearchQuery = q,
                Sort = sort,
                Page = page
            },
            // Genres and moods for filter dropdowns — can be populated later
            // when a dedicated reference data service is introduced.
            Genres = [],
            Moods = []
        };

        ViewData["Title"] = "Albums";
        ViewData["MetaDescription"] = "Browse the complete album catalog — filter by genre, mood, year, and more.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Album detail page (9.3).
    /// Displays full album information including tracklist, credits, awards, etc.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Album detail requested: culture={Culture}, slug={Slug}", culture, slug);

        var album = await _albumQueryService.GetAlbumBySlugAsync(
            slug,
            culture,
            cancellationToken: cancellationToken);

        if (album is null)
        {
            _logger.LogWarning("Album not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        var viewModel = new AlbumDetailViewModel
        {
            Album = album,
            Culture = culture
        };

        ViewData["Title"] = album.Title;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(album.Description)
            ? album.Description.Length > 200
                ? album.Description[..200] + "..."
                : album.Description
            : $"Album: {album.Title}";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "music.album";
        ViewData["OgImage"] = album.CoverUrl;
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/albums/{slug}";
        ViewData["TwitterCard"] = album.CoverUrl is not null ? "summary_large_image" : "summary";

        return View(viewModel);
    }
}
