using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for track listing and detail pages.
/// Routes: /{culture}/tracks and /{culture}/tracks/{slug}
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/tracks")]
public sealed class TracksController : Controller
{
    private readonly ITrackQueryService _trackQueryService;
    private readonly ILogger<TracksController> _logger;

    public TracksController(
        ITrackQueryService trackQueryService,
        ILogger<TracksController> logger)
    {
        _trackQueryService = trackQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Track listing page (9.4).
    /// Supports filtering by genre, artist, instrumental flag, and search text.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        string? genre = null,
        string? artist = null,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Track list requested: culture={Culture}, page={Page}, genre={Genre}, artist={Artist}, q={Q}",
            culture, page, genre, artist, q);

        var result = await _trackQueryService.GetTracksAsync(
            culture,
            page,
            pageSize: 24,
            genre: genre,
            artist: artist,
            q: q,
            cancellationToken: cancellationToken);

        var viewModel = new TrackListViewModel
        {
            Items = result,
            Culture = culture,
            CurrentFilters = new TrackListFilterViewModel
            {
                GenreSlug = genre,
                ArtistSlug = artist,
                SearchQuery = q,
                Page = page
            }
        };

        ViewData["Title"] = "Tracks";
        ViewData["MetaDescription"] = "Browse the complete track catalog — filter by genre, artist, and more.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Track detail page (9.5).
    /// Displays full track information including album appearances, artists, lyrics, etc.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Track detail requested: culture={Culture}, slug={Slug}", culture, slug);

        var track = await _trackQueryService.GetTrackBySlugAsync(
            slug,
            culture,
            cancellationToken: cancellationToken);

        if (track is null)
        {
            _logger.LogWarning("Track not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        // Determine lyrics section visibility based on business rules (section 34)
        bool lyricsSectionVisible;
        string? lyricsMessage = null;

        if (track.IsInstrumental)
        {
            lyricsSectionVisible = false;
            lyricsMessage = "This track is instrumental.";
        }
        else
        {
            var lyricsAvailability = track.LyricsAvailabilityName?.ToUpperInvariant() ?? "NONE";
            switch (lyricsAvailability)
            {
                case "PUBLIC":
                    lyricsSectionVisible = true;
                    break;
                case "REGISTERED":
                    lyricsSectionVisible = User.Identity?.IsAuthenticated == true;
                    lyricsMessage = lyricsSectionVisible ? null : "Lyrics are available to registered users.";
                    break;
                case "REQUEST":
                    lyricsSectionVisible = false;
                    lyricsMessage = "Lyrics are available upon request.";
                    break;
                case "RESTRICTED":
                    lyricsSectionVisible = User.HasClaim("Permission", "CanViewRestrictedLyrics");
                    lyricsMessage = lyricsSectionVisible ? null : "Lyrics are restricted.";
                    break;
                case "NONE":
                default:
                    lyricsSectionVisible = false;
                    lyricsMessage = "No lyrics available for this track.";
                    break;
            }
        }

        var viewModel = new TrackDetailViewModel
        {
            Track = track,
            LyricsSectionVisible = lyricsSectionVisible,
            LyricsMessage = lyricsMessage,
            Culture = culture
        };

        ViewData["Title"] = track.Title;
        ViewData["MetaDescription"] = $"Track: {track.Title}";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "music.song";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/tracks/{slug}";

        return View(viewModel);
    }
}
