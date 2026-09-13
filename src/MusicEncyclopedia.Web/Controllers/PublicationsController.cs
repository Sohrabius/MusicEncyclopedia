using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for publication listing and detail pages.
/// Publications are person-authored works (books, poetry collections, liner notes...).
/// Routes: /{culture}/publications
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/publications")]
public sealed class PublicationsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<PublicationsController> _logger;

    public PublicationsController(IDbConnection db, ILogger<PublicationsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 24;
        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM Publication p WHERE p.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                p.PublicationId,
                p.Title,
                p.Slug,
                pt.Name AS PublicationTypeName,
                cp.Name AS PublisherName,
                p.PublicationDate,
                p.ISBN
            FROM Publication p
            LEFT JOIN PublicationType pt ON p.PublicationTypeId = pt.PublicationTypeId
            LEFT JOIN Company cp ON p.PublisherId = cp.CompanyId
            WHERE p.IsDeleted = 0
            ORDER BY p.PublicationDate DESC, p.Title
            " + SqlDialect.Pagination();

        var items = (await _db.QueryAsync<PublicationListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();
        var result = MusicEncyclopedia.Core.DTOs.PagedResult<PublicationListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new PublicationListViewModel { Items = result, Culture = culture };

        ViewData["Title"] = "Publications";
        ViewData["MetaDescription"] = "Browse publications — poetry collections, books, and other authored works in the encyclopedia.";

        return View(viewModel);
    }

    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        var publicationSql = @"
            SELECT 
                p.PublicationId,
                p.Title,
                p.Slug,
                pt.Name AS PublicationTypeName,
                cp.Name AS PublisherName,
                p.PublicationDate,
                p.ISBN,
                pe.FullName AS AuthorName,
                pe.Slug AS AuthorSlug
            FROM Publication p
            LEFT JOIN PublicationType pt ON p.PublicationTypeId = pt.PublicationTypeId
            LEFT JOIN Company cp ON p.PublisherId = cp.CompanyId
            LEFT JOIN Person pe ON p.PersonId = pe.PersonId
            WHERE p.Slug = @Slug AND p.IsDeleted = 0";

        var publication = await _db.QuerySingleOrDefaultAsync<PublicationDetailDto>(publicationSql, new { Slug = slug });

        if (publication is null)
        {
            _logger.LogWarning("Publication not found: slug={Slug}", slug);
            return NotFound();
        }

        // Poems collected in this publication
        var poemsSql = @"
            SELECT 
                po.PoemId,
                po.Title,
                po.Slug
            FROM Poem po
            WHERE po.PublicationId = @PublicationId AND po.IsDeleted = 0
            ORDER BY po.Title";

        var poems = (await _db.QueryAsync<PublicationPoemDto>(poemsSql, new { PublicationId = publication.PublicationId })).ToList();

        publication = publication with { Poems = poems };

        var viewModel = new PublicationDetailViewModel { Publication = publication, Culture = culture };

        ViewData["Title"] = publication.Title;
        ViewData["MetaDescription"] = $"Publication: {publication.Title}"
            + (!string.IsNullOrWhiteSpace(publication.AuthorName) ? $" by {publication.AuthorName}" : "");
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/publications/{slug}";

        return View(viewModel);
    }
}
