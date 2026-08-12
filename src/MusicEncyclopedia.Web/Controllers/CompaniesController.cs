using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for company listing and detail pages.
/// Routes: /{culture}/companies and /{culture}/companies/{slug}
/// Spec references: 8.1 (routes), 9.7 (detail page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/companies")]
public sealed class CompaniesController : Controller
{
    private readonly ICompanyQueryService _companyQueryService;
    private readonly ILogger<CompaniesController> _logger;

    public CompaniesController(
        ICompanyQueryService companyQueryService,
        ILogger<CompaniesController> logger)
    {
        _companyQueryService = companyQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Company listing page.
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
            "Company list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        var result = await _companyQueryService.GetCompaniesAsync(
            culture,
            page,
            pageSize: 24,
            q: q,
            cancellationToken: cancellationToken);

        var viewModel = new CompanyListViewModel
        {
            Items = result,
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "Companies";
        ViewData["MetaDescription"] = "Browse the complete catalog of companies — record labels, publishers, distributors, and more.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Company detail page (9.7).
    /// Displays full company information including history, albums by role, track credits, etc.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Company detail requested: culture={Culture}, slug={Slug}", culture, slug);

        var company = await _companyQueryService.GetCompanyBySlugAsync(
            slug,
            culture,
            cancellationToken: cancellationToken);

        if (company is null)
        {
            _logger.LogWarning("Company not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        var viewModel = new CompanyDetailViewModel
        {
            Company = company,
            Culture = culture
        };

        // Attempt to extract a title from the dynamic company object for SEO
        string? companyTitle = null;
        try { companyTitle = (string?)((dynamic)company).Name; } catch { /* ignore */ }

        ViewData["Title"] = companyTitle ?? "Company";
        ViewData["MetaDescription"] = companyTitle is not null
            ? $"Profile: {companyTitle}"
            : "Company detail page";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/companies/{slug}";

        return View(viewModel);
    }
}
