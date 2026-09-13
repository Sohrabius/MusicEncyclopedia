using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Controllers.Api;

/// <summary>
/// Public API controller for people resources.
/// Spec 11.1 — People endpoints.
/// </summary>
public sealed class PeopleApiController : BaseApiController
{
    private readonly IPersonQueryService _personQueryService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PeopleApiController> _logger;

    public PeopleApiController(
        IPersonQueryService personQueryService,
        IConfiguration configuration,
        ILogger<PeopleApiController> logger)
    {
        _personQueryService = personQueryService;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/v1/people — paginated people listing.
    /// Supports ?page, ?pageSize, ?q.
    /// </summary>
    [HttpGet("people")]
    public async Task<IActionResult> GetPeople(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 24,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            return BadRequestResult("page", "MinValue", "Page must be 1 or greater.");
        if (pageSize is < 1 or > 100)
            return BadRequestResult("pageSize", "OutOfRange", "PageSize must be between 1 and 100.");

        var result = await _personQueryService.GetPeopleAsync(
            culture: "en",
            page: page,
            pageSize: pageSize,
            q: q,
            cancellationToken: cancellationToken);

        return OkListResult(result);
    }

    /// <summary>
    /// GET /api/v1/people/{slug} — person detail.
    /// </summary>
    [HttpGet("people/{slug}")]
    public async Task<IActionResult> GetPersonBySlug(
        string slug,
        CancellationToken cancellationToken = default)
    {
        var person = await _personQueryService.GetPersonBySlugAsync(slug, "en", cancellationToken);
        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        return OkResult(person);
    }

    /// <summary>
    /// GET /api/v1/people/{slug}/albums — albums associated with a person.
    /// </summary>
    [HttpGet("people/{slug}/albums")]
    public async Task<IActionResult> GetPersonAlbums(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        // First resolve person by slug
        var person = await connection.QueryFirstOrDefaultAsync(
            "SELECT PersonId, EntityId FROM Person WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        const string sql = @"
            SELECT DISTINCT
                a.AlbumId,
                a.Slug,
                a.Title,
                a.OriginalTitle,
                a.EnglishTitle,
                ac.Name AS CategoryName,
                a.ReleaseDate,
                a.DurationSeconds,
                m.Url AS CoverUrl
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN Album a ON a.EntityId = e.EntityId
            LEFT JOIN AlbumCategory ac ON ac.AlbumCategoryId = a.AlbumCategoryId
            LEFT JOIN Media m ON m.MediaId = a.CoverMediaId
            WHERE c.PersonId = @PersonId
              AND et.Code = 'Album'
              AND a.IsDeleted = 0
            ORDER BY a.ReleaseDate DESC";

        var albums = (await connection.QueryAsync(sql, new { PersonId = person.PersonId })).AsList();
        return OkListResult(albums);
    }

    /// <summary>
    /// GET /api/v1/people/{slug}/tracks — tracks associated with a person.
    /// </summary>
    [HttpGet("people/{slug}/tracks")]
    public async Task<IActionResult> GetPersonTracks(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var person = await connection.QueryFirstOrDefaultAsync(
            "SELECT PersonId, EntityId FROM Person WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        const string sql = @"
            SELECT DISTINCT
                t.TrackId,
                t.Slug,
                t.Title,
                t.OriginalTitle,
                t.EnglishTitle,
                t.DurationSeconds,
                t.IsInstrumental,
                t.IsExplicit
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            INNER JOIN Track t ON t.EntityId = e.EntityId
            WHERE c.PersonId = @PersonId
              AND et.Code = 'Track'
              AND t.IsDeleted = 0
            ORDER BY t.Title";

        var tracks = (await connection.QueryAsync(sql, new { PersonId = person.PersonId })).AsList();
        return OkListResult(tracks);
    }

    /// <summary>
    /// GET /api/v1/people/{slug}/credits — credits for a person.
    /// </summary>
    [HttpGet("people/{slug}/credits")]
    public async Task<IActionResult> GetPersonCredits(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var person = await connection.QueryFirstOrDefaultAsync(
            "SELECT PersonId, EntityId FROM Person WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                c.CreditId,
                et.Code AS EntityTypeCode,
                c.EntityId,
                cr.Name AS RoleName,
                cr.Code AS RoleCode,
                i.Name AS InstrumentName,
                c.DisplayOrder,
                c.IsPrimary,
                c.Notes
            FROM Credit c
            INNER JOIN Entity e ON e.EntityId = c.EntityId
            INNER JOIN EntityType et ON et.EntityTypeId = e.EntityTypeId
            LEFT JOIN CreditRole cr ON cr.CreditRoleId = c.CreditRoleId
            LEFT JOIN Instrument i ON i.InstrumentId = c.InstrumentId
            WHERE c.PersonId = @PersonId
            ORDER BY c.DisplayOrder";

        var credits = (await connection.QueryAsync(sql, new { PersonId = person.PersonId })).AsList();
        return OkListResult(credits);
    }

    /// <summary>
    /// GET /api/v1/people/{slug}/instruments — instruments played by a person.
    /// </summary>
    [HttpGet("people/{slug}/instruments")]
    public async Task<IActionResult> GetPersonInstruments(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var person = await connection.QueryFirstOrDefaultAsync(
            "SELECT PersonId, EntityId FROM Person WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        const string sql = @"
            SELECT DISTINCT
                i.InstrumentId,
                i.Slug,
                i.Name,
                i.Description,
                inf.Name AS InstrumentFamilyName
            FROM MusicianInstrument mi
            INNER JOIN Instrument i ON i.InstrumentId = mi.InstrumentId
            LEFT JOIN InstrumentFamily inf ON inf.InstrumentFamilyId = i.InstrumentFamilyId
            WHERE mi.PersonId = @PersonId AND i.IsDeleted = 0
            UNION
            SELECT DISTINCT
                i.InstrumentId,
                i.Slug,
                i.Name,
                i.Description,
                inf.Name AS InstrumentFamilyName
            FROM Credit c
            INNER JOIN Instrument i ON i.InstrumentId = c.InstrumentId
            LEFT JOIN InstrumentFamily inf ON inf.InstrumentFamilyId = i.InstrumentFamilyId
            WHERE c.PersonId = @PersonId AND i.IsDeleted = 0";

        var instruments = (await connection.QueryAsync(sql, new { PersonId = person.PersonId })).AsList();
        return OkListResult(instruments);
    }

    /// <summary>
    /// GET /api/v1/people/{slug}/poems — poems written by a person (if poet).
    /// </summary>
    [HttpGet("people/{slug}/poems")]
    public async Task<IActionResult> GetPersonPoems(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var person = await connection.QueryFirstOrDefaultAsync(
            "SELECT PersonId, EntityId FROM Person WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                p.PoemId,
                p.Title,
                p.Slug,
                p.CanonicalText,
                pub.Title AS PublicationTitle,
                pub.Slug AS PublicationSlug
            FROM Poem p
            LEFT JOIN Publication pub ON pub.PublicationId = p.PublicationId
            WHERE p.PersonId = @PersonId AND p.IsDeleted = 0
            ORDER BY p.Title";

        var poems = (await connection.QueryAsync(sql, new { PersonId = person.PersonId })).AsList();
        return OkListResult(poems);
    }

    /// <summary>
    /// GET /api/v1/people/{slug}/media — media for a person.
    /// </summary>
    [HttpGet("people/{slug}/media")]
    public async Task<IActionResult> GetPersonMedia(
        string slug,
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));

        var person = await connection.QueryFirstOrDefaultAsync(
            "SELECT PersonId, EntityId FROM Person WHERE Slug = @Slug AND IsDeleted = 0",
            new { Slug = slug });

        if (person is null)
            return NotFoundResult($"Person with slug '{slug}' not found.");

        const string sql = @"
            SELECT
                m.MediaId,
                m.Url,
                m.ThumbnailUrl300,
                mt.Name AS MediaType,
                mrt.Name AS MediaRole,
                ma.IsPrimary,
                m.Width,
                m.Height
            FROM MediaAssignment ma
            INNER JOIN Media m ON m.MediaId = ma.MediaId
            INNER JOIN MediaType mt ON mt.MediaTypeId = m.MediaTypeId
            LEFT JOIN MediaRoleType mrt ON mrt.MediaRoleTypeId = ma.MediaRoleTypeId
            WHERE ma.EntityTypeId = (SELECT EntityTypeId FROM EntityType WHERE Code = 'Person')
              AND ma.EntityId = @EntityId
              AND m.IsDeleted = 0";

        var media = (await connection.QueryAsync(sql, new { EntityId = person.EntityId })).AsList();

        return OkListResult(media);
    }
}
