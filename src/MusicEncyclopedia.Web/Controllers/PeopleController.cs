using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for person listing and detail pages.
/// Routes: /{culture}/people and /{culture}/people/{slug}
/// Spec references: 8.1 (routes), 9.6 (detail page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/people")]
public sealed class PeopleController : Controller
{
    private readonly IPersonQueryService _personQueryService;
    private readonly ILogger<PeopleController> _logger;

    public PeopleController(
        IPersonQueryService personQueryService,
        ILogger<PeopleController> logger)
    {
        _personQueryService = personQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Person listing page.
    /// Supports search via query string.
    /// </summary>
    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        string? q = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug(
            "Person list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        var result = await _personQueryService.GetPeopleAsync(
            culture,
            page,
            pageSize: 24,
            q: q,
            cancellationToken: cancellationToken);

        var viewModel = new PersonListViewModel
        {
            Items = result,
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "People";
        ViewData["MetaDescription"] = "Browse the complete catalog of people — artists, musicians, poets, composers, and more.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Person detail page (9.6).
    /// Displays full person information including biography, instruments, roles,
    /// career timeline, discography and track contributions (4.1, 4.2).
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        string? albumCategory = null,
        int? albumYear = null,
        string? albumRole = null,
        string? albumInstrument = null,
        string? contributionRole = null,
        string? contributionInstrument = null,
        string? contributionAlbum = null,
        string? contributionGenre = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Person detail requested: culture={Culture}, slug={Slug}", culture, slug);

        var person = await _personQueryService.GetPersonBySlugAsync(
            slug,
            culture,
            cancellationToken: cancellationToken);

        if (person is null)
        {
            _logger.LogWarning("Person not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        var viewModel = new PersonDetailViewModel
        {
            Person = person,
            Culture = culture,
            AlbumCategory = albumCategory,
            AlbumYear = albumYear,
            AlbumRole = albumRole,
            AlbumInstrument = albumInstrument,
            ContributionRole = contributionRole,
            ContributionInstrument = contributionInstrument,
            ContributionAlbum = contributionAlbum,
            ContributionGenre = contributionGenre
        };

        ViewData["Title"] = person.FullName;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(person.FullName)
            ? $"Profile: {person.FullName}"
            : "Person detail page";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "profile";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/people/{slug}";

        return View(viewModel);
    }
}
