using System.Data;
using Microsoft.AspNetCore.Mvc;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Web.ViewModels.Public;

namespace MusicEncyclopedia.Web.Controllers;

/// <summary>
/// Public controller for genre listing and detail pages.
/// Routes: /{culture}/genres and /{culture}/genres/{slug}
/// Spec references: 8.1 (routes), 9.8 (detail page)
/// </summary>
[Route("{culture:regex(^(fa)$)}/genres")]
public sealed class GenresController : Controller
{
    private readonly IDbConnection _db;
    private readonly ILogger<GenresController> _logger;

    public GenresController(
        IDbConnection db,
        ILogger<GenresController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Genre listing page (9.8).
    /// Shows all genres as a simple paginated list.
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
            "Genre list requested: culture={Culture}, page={Page}, q={Q}",
            culture, page, q);

        const int pageSize = 24;

        var offset = (page - 1) * pageSize;

        var countSql = "SELECT COUNT(*) FROM Genre WHERE IsDeleted = 0";
        var totalItems = await _db.ExecuteScalarAsync<int>(countSql);

        var itemsSql = q is null
            ? "SELECT GenreId, Name, Slug FROM Genre WHERE IsDeleted = 0 ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY"
            : "SELECT GenreId, Name, Slug FROM Genre WHERE IsDeleted = 0 AND (Name LIKE @Q OR Description LIKE @Q) ORDER BY Name OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var items = await _db.QueryAsync<NamedLinkDto>(
            itemsSql,
            new { Offset = offset, PageSize = pageSize, Q = q != null ? $"%{q}%" : null });

        var viewModel = new GenreListViewModel
        {
            Items = PagedResult<NamedLinkDto>.Create(
                items.AsList(), page, pageSize, totalItems),
            SearchQuery = q,
            Culture = culture
        };

        ViewData["Title"] = "Genres";
        ViewData["MetaDescription"] = "Browse all music genres — from classical and jazz to electronic and world music.";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";

        return View(viewModel);
    }

    /// <summary>
    /// Genre detail page (9.8).
    /// Displays full genre information including description, parent/related genres,
    /// albums, tracks, associated artists, media, and references.
    /// </summary>
    [HttpGet]
    [Route("{slug}")]
    [ResponseCache(Duration = 600)]
    public async Task<IActionResult> Detail(
        string culture,
        string slug,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Genre detail requested: culture={Culture}, slug={Slug}", culture, slug);

        // 1. Main genre
        var genre = await _db.QueryFirstOrDefaultAsync<(int GenreId, string Name, string Slug, string? Description, int? ParentGenreId)>(
            "SELECT GenreId, Name, Slug, Description, ParentGenreId FROM Genre WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (genre == default)
        {
            _logger.LogWarning("Genre not found: slug={Slug}, culture={Culture}", slug, culture);
            return NotFound();
        }

        var viewModel = new GenreDetailViewModel
        {
            GenreId = genre.GenreId,
            Name = genre.Name,
            Slug = genre.Slug,
            Description = genre.Description,
            Culture = culture
        };

        // 2. Parent genre
        if (genre.ParentGenreId.HasValue)
        {
            var parent = await _db.QueryFirstOrDefaultAsync<(string Name, string Slug)>(
                "SELECT Name, Slug FROM Genre WHERE GenreId = @Id AND IsDeleted = 0",
                new { Id = genre.ParentGenreId.Value });

            if (parent != default)
            {
                viewModel.ParentGenre = new GenreParentDto { Name = parent.Name, Slug = parent.Slug };
            }
        }

        // 3. Child genres
        var children = await _db.QueryAsync<(string Name, string Slug)>(
            "SELECT Name, Slug FROM Genre WHERE ParentGenreId = @Id AND IsDeleted = 0 ORDER BY Name",
            new { Id = genre.GenreId });

        viewModel.ChildGenres = children.Select(c => new GenreChildDto { Name = c.Name, Slug = c.Slug }).ToList();

        // 4. Albums in genre
        var albums = await _db.QueryAsync<(int AlbumId, string Title, string Slug, DateOnly? ReleaseDate)>(
            @"SELECT a.AlbumId, a.Title, a.Slug, a.ReleaseDate
              FROM Album a
              INNER JOIN AlbumGenre ag ON a.AlbumId = ag.AlbumId
              WHERE ag.GenreId = @GenreId AND a.IsDeleted = 0
              ORDER BY a.ReleaseDate DESC",
            new { GenreId = genre.GenreId });

        viewModel.Albums = albums.Select(a => new GenreAlbumDto
        {
            AlbumId = a.AlbumId,
            Title = a.Title,
            Slug = a.Slug,
            ReleaseDate = a.ReleaseDate
        }).ToList();

        // 5. Tracks in genre
        var tracks = await _db.QueryAsync<(int TrackId, string Title, string Slug, int? DurationSeconds)>(
            @"SELECT t.TrackId, t.Title, t.Slug, t.DurationSeconds
              FROM Track t
              INNER JOIN TrackGenre tg ON t.TrackId = tg.TrackId
              WHERE tg.GenreId = @GenreId AND t.IsDeleted = 0
              ORDER BY t.Title",
            new { GenreId = genre.GenreId });

        viewModel.Tracks = tracks.Select(t => new GenreTrackDto
        {
            TrackId = t.TrackId,
            Title = t.Title,
            Slug = t.Slug,
            DurationSeconds = t.DurationSeconds
        }).ToList();

        // 6. Artists associated (people with credits on albums or tracks in this genre)
        var entityTypeIds = await GetEntityTypeIdsAsync(_db, ["Album", "Track"]);

        var artists = await _db.QueryAsync<(int PersonId, string Name, string Slug)>(
            @"SELECT DISTINCT p.PersonId, p.FullName AS Name, p.Slug
              FROM Person p
              INNER JOIN Credit c ON p.PersonId = c.PersonId
              WHERE (
                  (c.EntityTypeId = @AlbumTypeId AND c.EntityId IN (SELECT AlbumId FROM AlbumGenre WHERE GenreId = @GenreId))
                  OR
                  (c.EntityTypeId = @TrackTypeId AND c.EntityId IN (SELECT TrackId FROM TrackGenre WHERE GenreId = @GenreId))
              )
              AND p.IsDeleted = 0
              ORDER BY p.FullName",
            new
            {
                GenreId = genre.GenreId,
                AlbumTypeId = entityTypeIds.GetValueOrDefault("Album", 0),
                TrackTypeId = entityTypeIds.GetValueOrDefault("Track", 0)
            });

        viewModel.Artists = artists.Select(a => new GenreArtistDto
        {
            PersonId = a.PersonId,
            Name = a.Name,
            Slug = a.Slug
        }).ToList();

        // 7. Media attached via Entity -> MediaAssignment
        var genreEntityId = await _db.ExecuteScalarAsync<int?>(
            "SELECT EntityId FROM Genre WHERE GenreId = @GenreId",
            new { GenreId = genre.GenreId });

        if (genreEntityId.HasValue)
        {
            var media = await _db.QueryAsync<(int MediaId, string Url, string? ThumbnailUrl, string? Description, string? MediaTypeName)>(
                @"SELECT m.MediaId,
                         COALESCE(m.Url, m.FilePath) AS Url,
                         m.ThumbnailUrl300 AS ThumbnailUrl,
                         ma.Description,
                         mt.Name AS MediaTypeName
                  FROM MediaAssignment ma
                  INNER JOIN Media m ON ma.MediaId = m.MediaId
                  LEFT JOIN MediaType mt ON m.MediaTypeId = mt.MediaTypeId
                  WHERE ma.EntityTypeId = @EntityTypeId AND ma.EntityId = @EntityId AND m.IsDeleted = 0
                  ORDER BY ma.DisplayOrder",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Genre", 0), EntityId = genreEntityId.Value });

            viewModel.Media = media.Select(m => new GenreMediaDto
            {
                MediaId = m.MediaId,
                Url = m.Url,
                ThumbnailUrl = m.ThumbnailUrl,
                Description = m.Description,
                MediaType = m.MediaTypeName
            }).ToList();

            // 8. Citations
            var citations = await _db.QueryAsync<(int CitationId, string? Quote, string? SourceName, string? SourceSlug, string? PageNumber, string? Url)>(
                @"SELECT c.CitationId, c.Quote, s.Title AS SourceName, s.Slug AS SourceSlug, c.PageNumber, c.Url
                  FROM Citation c
                  LEFT JOIN [Source] s ON c.SourceId = s.SourceId
                  WHERE c.EntityTypeId = @EntityTypeId AND c.EntityId = @EntityId
                  ORDER BY c.CitationId",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Genre", 0), EntityId = genreEntityId.Value });

            viewModel.Citations = citations.Select(c => new GenreCitationDto
            {
                CitationId = c.CitationId,
                Quote = c.Quote,
                SourceName = c.SourceName,
                SourceSlug = c.SourceSlug,
                PageNumber = c.PageNumber,
                Url = c.Url
            }).ToList();

            // 9. External links
            var links = await _db.QueryAsync<(string Url, string? Title, string? LinkTypeCode)>(
                @"SELECT el.Url, el.Title, lt.Code AS LinkTypeCode
                  FROM EntityLink el
                  LEFT JOIN LinkType lt ON el.LinkTypeId = lt.LinkTypeId
                  WHERE el.EntityTypeId = @EntityTypeId AND el.EntityId = @EntityId AND el.IsDeleted = 0
                  ORDER BY el.EntityLinkId",
                new { EntityTypeId = entityTypeIds.GetValueOrDefault("Genre", 0), EntityId = genreEntityId.Value });

            viewModel.Links = links.Select(l => new GenreLinkDto
            {
                Url = l.Url,
                Title = l.Title,
                LinkType = l.LinkTypeCode
            }).ToList();
        }

        ViewData["Title"] = genre.Name;
        ViewData["MetaDescription"] = !string.IsNullOrWhiteSpace(genre.Description)
            ? (genre.Description.Length > 200 ? genre.Description[..200] + "..." : genre.Description)
            : $"Genre: {genre.Name}";
        ViewData["Robots"] = "index, follow";
        ViewData["OgType"] = "website";
        ViewData["CanonicalUrl"] = $"{Request.Scheme}://{Request.Host}/{culture}/genres/{slug}";

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
