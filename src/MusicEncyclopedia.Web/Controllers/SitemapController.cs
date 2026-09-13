using System.Data;
using System.Xml.Linq;
using Dapper;
using Microsoft.AspNetCore.Mvc;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Generates the XML sitemap at /sitemap.xml for all public entity URLs
/// across all four supported cultures (fa, en, ar, fr).
/// Cached for 24 hours.
/// </summary>
[Route("")]
public sealed class SitemapController : Controller
{
    private readonly IDbConnection _db;
    private readonly string _baseUrl;
    private readonly ILogger<SitemapController> _logger;

    public SitemapController(
        IDbConnection db,
        IConfiguration configuration,
        ILogger<SitemapController> logger)
    {
        _db = db;
        _baseUrl = (configuration.GetValue<string>("Site:BaseUrl") ?? "https://example.com").TrimEnd('/');
        _logger = logger;
    }

    private static readonly string[] Cultures = ["fa", "en", "ar", "fr"];

    /// <summary>
    /// Generates the sitemap.xml with all public entity URLs.
    /// Route: GET /sitemap.xml
    /// </summary>
    [Route("/sitemap.xml")]
    public async Task<IActionResult> Index()
    {
        var urls = new List<SitemapUrl>();

        // ────────────────────────────────────────────────────────────
        // Home page for each culture (priority: 1.0, daily)
        // ────────────────────────────────────────────────────────────
        foreach (var culture in Cultures)
        {
            urls.Add(new SitemapUrl
            {
                Location = $"{_baseUrl}/{culture}",
                Priority = 1.0m,
                ChangeFreq = "daily",
                LastMod = DateTime.UtcNow.ToString("yyyy-MM-dd")
            });
        }

        // ────────────────────────────────────────────────────────────
        // Detail entity pages — query each table for active slugs
        // ────────────────────────────────────────────────────────────
        await AddEntityUrlsAsync("Album", "albums", 0.8m, "weekly", urls);
        await AddEntityUrlsAsync("Track", "tracks", 0.8m, "weekly", urls);
        await AddEntityUrlsAsync("Person", "people", 0.8m, "weekly", urls);
        await AddEntityUrlsAsync("Company", "companies", 0.7m, "monthly", urls);
        await AddEntityUrlsAsync("Genre", "genres", 0.6m, "monthly", urls);
        await AddEntityUrlsAsync("Mood", "moods", 0.5m, "monthly", urls);
        await AddEntityUrlsAsync("Instrument", "instruments", 0.5m, "monthly", urls);
        await AddEntityUrlsAsync("Poem", "poems", 0.7m, "monthly", urls);
        await AddEntityUrlsAsync("SungVersion", "sung-versions", 0.7m, "monthly", urls);
        await AddEntityUrlsAsync("RecordingSession", "sessions", 0.6m, "monthly", urls);
        await AddEntityUrlsAsync("PerformanceEvent", "events", 0.6m, "monthly", urls);
        await AddEntityUrlsAsync("Location", "locations", 0.5m, "monthly", urls);
        await AddEntityUrlsAsync("Award", "awards", 0.6m, "monthly", urls);
        await AddEntityUrlsAsync("Chart", "charts", 0.5m, "monthly", urls);
        await AddEntityUrlsAsync("Source", "sources", 0.5m, "monthly", urls);
        await AddEntityUrlsAsync("Publication", "publications", 0.6m, "monthly", urls);
        await AddEntityUrlsAsync("Tag", "tags", 0.4m, "monthly", urls);

        // ────────────────────────────────────────────────────────────
        // List / index pages for each entity type (priority: 0.6, monthly)
        // ────────────────────────────────────────────────────────────
        var listPages = new (string route, decimal priority)[]
        {
            ("albums", 0.6m),
            ("tracks", 0.6m),
            ("people", 0.6m),
            ("companies", 0.6m),
            ("genres", 0.6m),
            ("moods", 0.6m),
            ("instruments", 0.6m),
            ("poems", 0.6m),
            ("sung-versions", 0.6m),
            ("sessions", 0.6m),
            ("events", 0.6m),
            ("locations", 0.6m),
            ("awards", 0.6m),
            ("charts", 0.6m),
            ("sources", 0.6m),
            ("publications", 0.6m),
            ("tags", 0.6m),
        };

        foreach (var (route, priority) in listPages)
        {
            foreach (var culture in Cultures)
            {
                urls.Add(new SitemapUrl
                {
                    Location = $"{_baseUrl}/{culture}/{route}",
                    Priority = priority,
                    ChangeFreq = "monthly"
                });
            }
        }

        // ────────────────────────────────────────────────────────────
        // Build the XML sitemap document
        // ────────────────────────────────────────────────────────────
        var xmlns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");

        var doc = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(xmlns + "urlset",
                urls.Select(u =>
                {
                    var elements = new List<XElement>
                    {
                        new XElement(xmlns + "loc", u.Location)
                    };

                    if (u.LastMod is not null)
                    {
                        elements.Add(new XElement(xmlns + "lastmod", u.LastMod));
                    }

                    elements.Add(new XElement(xmlns + "changefreq", u.ChangeFreq));
                    elements.Add(new XElement(xmlns + "priority", u.Priority.ToString("0.0")));

                    return new XElement(xmlns + "url", elements);
                })
            )
        );

        using var sw = new StringWriter();
        doc.Save(sw);

        _logger.LogInformation("Sitemap generated with {UrlCount} URLs", urls.Count);

        return Content(sw.ToString(), "application/xml", System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// Queries a single entity table for active slugs and adds URLs
    /// for all four cultures.
    /// </summary>
    private async Task AddEntityUrlsAsync(
        string tableName,
        string routeSegment,
        decimal priority,
        string changeFreq,
        List<SitemapUrl> urls)
    {
        try
        {
            var rows = await _db.QueryAsync<(string slug, DateTime lastMod)>(
                $"SELECT Slug, COALESCE(ModifiedAt, CreatedAt) AS LastMod FROM [{tableName}] WHERE IsDeleted = 0");

            foreach (var row in rows)
            {
                foreach (var culture in Cultures)
                {
                    urls.Add(new SitemapUrl
                    {
                        Location = $"{_baseUrl}/{culture}/{routeSegment}/{row.slug}",
                        Priority = priority,
                        ChangeFreq = changeFreq,
                        LastMod = row.lastMod.ToString("yyyy-MM-dd")
                    });
                }
            }

            _logger.LogDebug("Added {Count} sitemap URLs from table {Table}", rows.Count(), tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to query table {Table} for sitemap", tableName);
        }
    }

    /// <summary>
    /// Internal model for a single sitemap URL entry.
    /// </summary>
    private sealed class SitemapUrl
    {
        public string Location { get; set; } = null!;
        public string? LastMod { get; set; }
        public string ChangeFreq { get; set; } = null!;
        public decimal Priority { get; set; }
    }
}
