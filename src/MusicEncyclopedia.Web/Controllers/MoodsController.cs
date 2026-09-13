using System.Data;
using MusicEncyclopedia.Services.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for mood listing and detail pages.
/// Routes: /{culture}/moods and /{culture}/moods/{slug}
/// Spec references: 8.1 (routes), 9.9 (detail page)
/// </summary>
[Route("{culture:regex(^(fa|en|ar|fr)$)}/moods")]
public sealed class MoodsController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<MoodsController> _logger;

    public MoodsController(
        IDbConnection db,
        ILogger<MoodsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Mood listing page (9.9).
    /// Shows all moods as a simple paginated list.
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
            "Mood list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        const int pageSize = 24;

        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM Mood WHERE IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var itemsSql = q is null
            ? "SELECT MoodId, Name, Slug FROM Mood WHERE IsDeleted = 0 ORDER BY Name " + SqlDialect.Pagination()
            : "SELECT MoodId, Name, Slug FROM Mood WHERE IsDeleted = 0 AND (Name LIKE @Q OR Description LIKE @Q) ORDER BY Name " + SqlDialect.Pagination();

        var items = await _db.QueryAsync<NamedLinkDto>(
            itemsSql,
            new { Offset = offset, PageSize = pageSize, Q = q != null ? $"%{q}%" : null });

        var viewModel = new MoodListViewModel
        {
            Items = PagedResult<NamedLinkDto>.Create(
                items.AsList(), page, pageSize, totalItems),
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "Moods";
        ViewData["MetaDescription"] = "Browse all music moods — from upbeat and energetic to calm and reflective.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Mood detail page (9.9).
    /// Displays full mood information including albums, tracks, media, and tags.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Mood detail requested: culture={Culture}, slug={Slug}", culture, slug);

        // 1. Main mood
        var mood = await _db.QueryFirstOrDefaultAsync<(int MoodId, string Name, string Slug, string? Description)>(
            "SELECT MoodId, Name, Slug, Description FROM Mood WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (mood == default)
        {
            _logger.LogWarning("Mood not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        var viewModel = new MoodDetailViewModel
        {
            MoodId = mood.MoodId,
            Name = mood.Name,
            Slug = mood.Slug,
            Description = mood.Description,
            Culture = culture
        };

        // 2. Albums with this mood
        var albums = await _db.QueryAsync<(int AlbumId, string Title, string Slug, DateOnly? ReleaseDate)>(
            @"SELECT a.AlbumId, a.Title, a.Slug, a.ReleaseDate
              FROM Album a
              INNER JOIN AlbumMood am ON a.AlbumId = am.AlbumId
              WHERE am.MoodId = @MoodId AND a.IsDeleted = 0
              ORDER BY a.ReleaseDate DESC",
            new { MoodId = mood.MoodId });

        viewModel.Albums = albums.Select(a => new MoodAlbumDto
        {
            AlbumId = a.AlbumId,
            Title = a.Title,
            Slug = a.Slug,
            ReleaseDate = a.ReleaseDate
        }).ToList();

        // 3. Tracks with this mood
        var tracks = await _db.QueryAsync<(int TrackId, string Title, string Slug, int? DurationSeconds)>(
            @"SELECT t.TrackId, t.Title, t.Slug, t.DurationSeconds
              FROM Track t
              INNER JOIN TrackMood tm ON t.TrackId = tm.TrackId
              WHERE tm.MoodId = @MoodId AND t.IsDeleted = 0
              ORDER BY t.Title",
            new { MoodId = mood.MoodId });

        viewModel.Tracks = tracks.Select(t => new MoodTrackDto
        {
            TrackId = t.TrackId,
            Title = t.Title,
            Slug = t.Slug,
            DurationSeconds = t.DurationSeconds
        }).ToList();

        // 4. Media attached via Entity -> MediaAssignment
        var entityTypeIds = await GetEntityTypeIdsAsync(_db, ["Mood", "Genre"]);

        var moodEntityId = await _db.ExecuteScalarAsync<int?>(
            "SELECT EntityId FROM Mood WHERE MoodId = @MoodId",
            new { MoodId = mood.MoodId });

        if (moodEntityId.HasValue)
        {
            var media = await _db.QueryAsync<(int MediaId, string Url, string? ThumbnailUrl, string? Description, string? MediaTypeName)>(
                @"SELECT m.MediaId,
                         COALESCE(m.Url, m.FilePath) AS Url,
                         m.ThumbnailUrl300 AS ThumbnailUrl,
                         mt.Name AS MediaTypeName
                  FROM MediaAssignment ma
                  INNER JOIN Media m ON ma.MediaId = m.MediaId
                  LEFT JOIN MediaType mt ON m.MediaTypeId = mt.MediaTypeId
                  WHERE ma.EntityTypeId = @EntityTypeId AND ma.EntityId = @EntityId AND m.IsDeleted = 0
                  ORDER BY ma.DisplayOrder",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Mood", 0), EntityId = moodEntityId.Value });

            viewModel.Media = media.Select(m => new MoodMediaDto
            {
                MediaId = m.MediaId,
                Url = m.Url,
                ThumbnailUrl = m.ThumbnailUrl,
                Description = m.Description,
                MediaType = m.MediaTypeName
            }).ToList();
        }

        // 5. Tags (via TagAssignment -> Entity)
        if (moodEntityId.HasValue)
        {
            var tags = await _db.QueryAsync<(string Name, string Slug)>(
                @"SELECT t.Name, t.Slug
                  FROM TagAssignment ta
                  INNER JOIN Tag t ON ta.TagId = t.TagId
                  WHERE ta.EntityTypeId = @EntityTypeId AND ta.EntityId = @EntityId AND t.IsDeleted = 0
                  ORDER BY t.Name",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Mood", 0), EntityId = moodEntityId.Value });

            viewModel.Tags = tags.Select(t => new MoodTagDto
            {
                Name = t.Name,
                Slug = t.Slug
            }).ToList();
        }

        ViewData["Title"] = mood.Name;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(mood.Description)
            ? (mood.Description.Length > 200 ? mood.Description[..200] + "..." : mood.Description)
            : $"Mood: {mood.Name}";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/moods/{slug}";

        return View(viewModel);
    }

    /// <summary>
    /// Returns a dictionary mapping entity type codes to their integer IDs.
    /// </summary>
    private static async Task<Dictionary<string, int>> GetEntityTypeIdsAsync(
        IDbConnection connection,
        string[] codes)
    {
        var rows = await connection.QueryAsync<(int EntityTypeId, string Code)>(
            "SELECT EntityTypeId, Code FROM EntityType WHERE Code IN @Codes",
            new { Codes = codes });

        return rows.ToDictionary(r => r.Code, r => r.EntityTypeId);
    }
}
