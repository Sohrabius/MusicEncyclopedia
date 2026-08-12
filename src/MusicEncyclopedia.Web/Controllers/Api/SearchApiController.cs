using Microsoft.AspNetCore.Mvc;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;
using MusicEncyclopedia.Web.ViewModels.Api;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for search.
/// Spec 11.1 — Search endpoint.
/// Uses ISearchService for full-text search.
/// </summary>
public sealed class SearchApiController : BaseApiController
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchApiController> _logger;

    public SearchApiController(
        ISearchService searchService,
        ILogger<SearchApiController> logger)
    {
        _searchService = searchService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/search — full-text search across all entity types.
    /// </summary>
    /// <param name="q">Search term (required).</param>
    /// <param name="type">Optional entity type filter (album, track, person, company, etc.).</param>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="pageSize">Results per page (default 20, max 100).</param>
    [HttpGet("search")]
    [ResponseCache(Duration = 60, VaryByQueryKeys = ["q", "type", "page", "pageSize"])]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        // Validate required query term
        if (string.IsNullOrWhiteSpace(q))
            return BadRequestResult("q", "Required", "Search term 'q' is required.");

        if (q!.Length > 200)
            return BadRequestResult("q", "MaxLength", "Search term must not exceed 200 characters.");

        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");

        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        var query = new SearchQuery
        {
            Q = q,
            EntityType = string.IsNullOrWhiteSpace(type) ? null : type,
            Culture = "en",
            Page = page,
            PageSize = pageSize
        };

        var result = await _searchService.SearchAsync(query, cancellationToken);

        if (result.Items.Count == 0)
            return Ok(ApiResponse<ApiListResponse<SearchResultDto>>.Ok(
                new ApiListResponse<SearchResultDto>
                {
                    Items = [],
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = 0,
                    TotalPages = 0
                },
                "No results found."));

        return OkListResult(result);
    }
}
