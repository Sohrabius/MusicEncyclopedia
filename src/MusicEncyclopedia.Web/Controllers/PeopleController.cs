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
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
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
    /// Displays full person information including biography, instruments, roles, media, etc.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
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
            Culture = culture
        };

        // Attempt to extract a title from the dynamic person object for SEO
        string? personTitle = null;
        try { personTitle = (string?)((dynamic)person).FullName ?? (string?)((dynamic)person).Name; } catch { /* ignore */ }

        ViewData["Title"] = personTitle ?? "Person";
        ViewData["MetaDescription"] = personTitle is not null
            ? $"Profile: {personTitle}"
            : "Person detail page";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "profile";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/people/{slug}";

        return View(viewModel);
    }
}
