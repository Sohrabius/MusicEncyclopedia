using Microsoft.AspNetCore.Mvc;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Home controller handling the main public landing page and error pages.
/// Route is culture-aware per the application's localization strategy.
/// </summary>
[Route("{culture:regex(^(fa)$)}")]
public sealed class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
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
    [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = ["*"])]
    public IActionResult Index(string culture)
    {
        _logger.LogDebug("Home page requested for culture: {Culture}", culture);

        var viewModel = new ViewModels.HomeViewModel
        {
            CurrentCulture = culture,
            MetaDescription = "Explore the complete encyclopedia of music — albums, tracks, artists, lyrics, and more."
        };

        return View(viewModel);
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

        return View(new ViewModels.ErrorViewModel
        {
            RequestId = requestId,
            ShowRequestId = !string.IsNullOrEmpty(requestId)
        });
    }
}
