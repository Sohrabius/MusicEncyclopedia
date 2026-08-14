using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for source listing and detail pages.
/// Routes: /{culture}/sources
/// Spec reference: 9.19
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/sources")]
public sealed class SourcesController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<SourcesController> _logger;

    public SourcesController(IDbConnection db, ILogger<SourcesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    [Route("")]
    [Route("Index")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = new[] { "*" }, VaryByHeader = "Accept-Language")]
    public async Task<IActionResult> Index(
        string culture,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        const int pageSize = 24;
        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM Source s WHERE s.IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var sql = @"
            SELECT 
                s.SourceId,
                s.Title,
                s.Slug,
                st.Name AS SourceTypeName,
                s.Author,
                cp.Name AS PublisherName,
                s.PublicationDate
            FROM Source s
            LEFT JOIN SourceType st ON s.SourceTypeId = st.SourceTypeId
            LEFT JOIN Company cp ON s.PublisherId = cp.CompanyId
            WHERE s.IsDeleted = 0
            ORDER BY s.Title
            " + SqlDialect.Pagination();

        var items = (await _db.QueryAsync<SourceListItemDto>(sql, new { Offset = offset, PageSize = pageSize })).ToList();
        var result = MusicEncyclopedia.Core.DTOs.PagedResult<SourceListItemDto>.Create(items.AsReadOnly(), page, pageSize, totalItems);

        var viewModel = new SourceListViewModel { Items = result, Culture = culture };

        ViewData["Title"] = "Sources";
        ViewData["MetaDescription"] = "Browse sources — books, articles, websites, and references used in the encyclopedia.";

        return View(viewModel);
    }

    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 60)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        var sourceSql = @"
            SELECT 
                s.SourceId,
                s.Title,
                s.Slug,
                st.Name AS SourceTypeName,
                s.Author,
                cp.Name AS PublisherName,
                s.PublicationDate,
                s.Url
            FROM Source s
            LEFT JOIN SourceType st ON s.SourceTypeId = st.SourceTypeId
            LEFT JOIN Company cp ON s.PublisherId = cp.CompanyId
            WHERE s.Slug = @Slug AND s.IsDeleted = 0";

        var source = await _db.QuerySingleOrDefaultAsync<SourceDetailDto>(sourceSql, new { Slug = slug });

        if (source is null)
        {
            _logger.LogWarning("Source not found: slug={Slug}", slug);
            return NotFound();
        }

        // Citations referencing this source
        var citationsSql = @"
            SELECT 
                c.CitationId,
                c.EntityTypeId,
                c.EntityId,
                c.FieldName,
                c.Quote,
                c.PageNumber
            FROM Citation c
            WHERE c.SourceId = @SourceId
            ORDER BY c.EntityTypeId, c.EntityId";

        var citations = (await _db.QueryAsync<SourceCitationDto>(citationsSql, new { SourceId = source.SourceId })).ToList();

        // Resolve entity titles
        for (int i = 0; i < citations.Count; i++)
        {
            var citation = citations[i];
            string? titleSql = citation.EntityTypeId switch
            {
                1 => "SELECT Title AS Name FROM Album WHERE EntityId = @Eid",
                2 => "SELECT Title AS Name FROM Track WHERE EntityId = @Eid",
                3 => "SELECT FullName AS Name FROM Person WHERE EntityId = @Eid",
                4 => "SELECT Name AS Name FROM Company WHERE EntityId = @Eid",
                5 => "SELECT Title AS Name FROM Poem WHERE EntityId = @Eid",
                8 => "SELECT Title AS Name FROM RecordingSession WHERE EntityId = @Eid",
                _ => null
            };

            if (titleSql is not null)
            {
                var entityName = await _db.QuerySingleOrDefaultAsync<string>(titleSql, new { Eid = citation.EntityId });
                citations[i] = citation with { EntityTitle = entityName ?? "Unknown" };
            }
        }

        source = source with { Citations = citations };

        var viewModel = new SourceDetailViewModel { Source = source, Culture = culture };

        ViewData["Title"] = source.Title;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(source.Author)
            ? $"Source: {source.Title} by {source.Author}"
            : $"Source: {source.Title}";
        ViewData["Robots"] = "index, follow";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/sources/{slug}";

        return View(viewModel);
    }
}
