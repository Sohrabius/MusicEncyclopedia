using Microsoft.AspNetCore.Mvc;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Serves the robots.txt file at /robots.txt with crawl rules
/// and the sitemap URL. Cached for 24 hours.
/// </summary>
[Route("")]
public sealed class RobotsController : Controller
{
    private readonly string _baseUrl;

    public RobotsController(IConfiguration configuration)
    {
        _baseUrl = (configuration.GetValue<string>("Site:BaseUrl") ?? "https://example.com").TrimEnd('/');
    }

    /// <summary>
    /// Returns robots.txt with crawl directives.
    /// Route: GET /robots.txt
    /// </summary>
    [Route("/robots.txt")]
    public IActionResult Index()
    {
        var robots = $@"User-agent: *
Allow: /
Disallow: /admin
Disallow: /api
Disallow: /auth

Sitemap: {_baseUrl}/sitemap.xml";

        return Content(robots, "text/plain", System.Text.Encoding.UTF8);
    }
}
