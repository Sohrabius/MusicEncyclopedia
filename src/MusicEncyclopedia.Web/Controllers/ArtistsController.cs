using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for artist listing pages.
/// Routes: /{culture}/artists
/// An artist is a Person with PersonType = MAIN_ARTIST.
/// Spec references: 8.1 (routes), 9.6 (person detail — reused for artist profiles)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/artists")]
public sealed class ArtistsController : Controller
{
    private readonly IPersonQueryService _personQueryService;
    private readonly ILogger<ArtistsController> _logger;

    public ArtistsController(
        IPersonQueryService personQueryService,
        ILogger<ArtistsController> logger)
    {
        _personQueryService = personQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Artist listing page — filtered to people with PersonType = MAIN_ARTIST.
    /// Reuses IPersonQueryService for consistency.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Artist list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        var result = await _personQueryService.GetPeopleAsync(
            culture,
            page,
            pageSize: 24,
            q: q,
            personType: "MAIN_ARTIST",
            cancellationToken: cancellationToken);

        var viewModel = new PersonListViewModel
        {
            Items = result,
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "Artists";
        ViewData["MetaDescription"] = "Browse the complete catalog of artists.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Artist profile — permanent redirect to the canonical person detail page,
    /// which renders the full profile for any person.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    public IActionResult Detail(string culture, string slug)
    {
        _logger.LogDebug("Artist detail requested: culture={Culture}, slug={Slug}", culture, slug);
        return RedirectToActionPermanent(nameof(PeopleController.Detail), "People", new { culture, slug });
    }
}
