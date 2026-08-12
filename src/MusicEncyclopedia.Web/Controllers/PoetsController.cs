using System.Data;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for poet listing and detail pages.
/// Routes: /{culture}/poets and /{culture}/poets/{slug}
/// A poet is a Person with PersonKind = "poet".
/// Spec references: 8.1 (routes), 9.11 (detail page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/poets")]
public sealed class PoetsController : Controller
{
    private readonly IPersonQueryService _personQueryService;
    private readonly IDbConnection _db;
    private readonly ILogger<PoetsController> _logger;

    public PoetsController(
        IPersonQueryService personQueryService,
        IDbConnection db,
        ILogger<PoetsController> logger)
    {
        _personQueryService = personQueryService;
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Poet listing page — filtered to Person with PersonKind = poet.
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
            "Poet list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        var result = await _personQueryService.GetPeopleAsync(
            culture,
            page,
            pageSize: 24,
            q: q,
            personType: "POET",
            cancellationToken: cancellationToken);

        var viewModel = new PoetListViewModel
        {
            Items = result,
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "Poets";
        ViewData["MetaDescription"] = "Browse the complete catalog of poets.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Poet detail page per spec 9.11.
    /// Displays poet info plus poems, publications, and sung versions.
    /// Uses Dapper directly for poet-specific related data.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Poet detail requested: culture={Culture}, slug={Slug}", culture, slug);

        // First, get the Person object using the existing query service
        var poet = await _personQueryService.GetPersonBySlugAsync(
            slug,
            culture,
            cancellationToken: cancellationToken);

        if (poet is null)
        {
            _logger.LogWarning("Poet (Person) not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        // Augment with poet-specific related data (poems, publications, sung versions)
        try
        {
            // Try to get the PersonId from the dynamic object
            int? personId = null;
            try { personId = (int)((dynamic)poet).PersonId; } catch { }
            try { personId ??= (int)((dynamic)poet).Id; } catch { }

            // Try to get EntityId as well
            int? entityId = null;
            try { entityId = (int)((dynamic)poet).EntityId; } catch { }

            IReadOnlyList<dynamic> poems = [];
            IReadOnlyList<dynamic> publications = [];
            IReadOnlyList<dynamic> sungVersions = [];

            if (personId.HasValue)
            {
                poems = await GetPoemsByPersonAsync(_db, personId.Value, cancellationToken);
                publications = await GetPublicationsByPersonAsync(_db, personId.Value, cancellationToken);
                sungVersions = await GetSungVersionsByPersonAsync(_db, personId.Value, cancellationToken);
            }

            // Build augmented poet object
            var augmented = new
            {
                Person = poet,
                Poems = poems,
                Publications = publications,
                SungVersions = sungVersions
            };

            var viewModel = new PoetDetailViewModel
            {
                Poet = augmented,
                Culture = culture
            };

            // Attempt to extract poet name for SEO
            string? poetName = null;
            try { poetName = (string?)((dynamic)poet).FullName ?? (string?)((dynamic)poet).Name; } catch { }

            ViewData["Title"] = poetName is not null ? $"{poetName} — Poet" : "Poet";
            ViewData["MetaDescription"] = poetName is not null
                ? $"Profile and works of poet {poetName}"
                : "Poet detail page";
            ViewData["Robots"] = "index, follow";
            ViewData["OgType"] = "profile";
            ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/poets/{slug}";

            return View(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load poet-specific data: slug={Slug}, culture={Culture}", slug, culture);

            // Fall back to showing just the person data
            var viewModel = new PoetDetailViewModel
            {
                Poet = new { Person = poet, Poems = Array.Empty<object>(), Publications = Array.Empty<object>(), SungVersions = Array.Empty<object>() },
                Culture = culture
            };

            ViewData["Title"] = "Poet";
            ViewData["Robots"] = "index, follow";

            return View(viewModel);
        }
    }

    // ────────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────────

    private static async Task<IReadOnlyList<dynamic>> GetPoemsByPersonAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT p.Slug, p.Title, p.CanonicalText, p.Book
            FROM Poem p
            WHERE p.PersonId = @PersonId AND p.IsDeleted = 0
            ORDER BY p.OriginalPublicationDate DESC, p.Title
            """;
        var results = await connection.QueryAsync(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetPublicationsByPersonAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT pub.Slug, pub.Title, pub.PublicationDate, pub.ISBN,
                   pt.Name AS PublicationTypeName
            FROM Publication pub
            LEFT JOIN PublicationType pt ON pt.PublicationTypeId = pub.PublicationTypeId
            WHERE pub.PersonId = @PersonId AND pub.IsDeleted = 0
            ORDER BY pub.PublicationDate DESC, pub.Title
            """;
        var results = await connection.QueryAsync(sql, new { PersonId = personId });
        return results.AsList();
    }

    private static async Task<IReadOnlyList<dynamic>> GetSungVersionsByPersonAsync(
        IDbConnection connection,
        int personId,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT sv.Slug, sv.Title, vs.Name AS VocalStyleName, sv.IsCanonical,
                   p.Slug AS PoemSlug, p.Title AS PoemTitle
            FROM SungVersion sv
            INNER JOIN Poem p ON p.PoemId = sv.PoemId AND p.IsDeleted = 0
            LEFT JOIN VocalStyle vs ON vs.VocalStyleId = sv.VocalStyleId
            WHERE p.PersonId = @PersonId AND sv.IsDeleted = 0
            ORDER BY sv.IsCanonical DESC, sv.Title
            """;
        var results = await connection.QueryAsync(sql, new { PersonId = personId });
        return results.AsList();
    }
}
