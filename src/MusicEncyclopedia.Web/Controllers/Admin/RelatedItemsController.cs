using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Generic AJAX related-items editor (spec 10.5/10.6).
/// Powers the "Genres", "Moods", "Languages", "Countries", "Companies", "Identifiers"
/// tabs on the album edit page and "Genres", "Moods", "Instruments", "Sung Versions",
/// "Related Tracks", "Version Types" tabs on the track edit page.
/// Route: /admin/related-items
/// </summary>
[Route("/admin/related-items")]
[Authorize(Policy = PermissionConstants.CanManageAlbums)]
public sealed class RelatedItemsController : AdminBaseController
{
    private const int AlbumEntityTypeId = 1;
    private const int TrackEntityTypeId = 2;

    private readonly AppDbContext _db;
    private readonly ILogger<RelatedItemsController> _logger;

    public RelatedItemsController(AppDbContext db, ILogger<RelatedItemsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET: Returns the editor partial for a given kind + entity.
    /// GET /admin/related-items/{kind}/{entityId}
    /// </summary>
    [HttpGet]
    [Route("{kind}/{entityId:int}")]
    public async Task<IActionResult> Index(
        string kind,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        RelatedItemsEditorViewModel? vm = kind switch
        {
            "album-genres" => await BuildAlbumGenreAsync(entityId, cancellationToken),
            "album-moods" => await BuildAlbumMoodAsync(entityId, cancellationToken),
            "album-languages" => await BuildAlbumLanguageAsync(entityId, cancellationToken),
            "album-countries" => await BuildAlbumCountryAsync(entityId, cancellationToken),
            "album-companies" => await BuildAlbumCompanyAsync(entityId, cancellationToken),
            "album-identifiers" => await BuildAlbumIdentifierAsync(entityId, cancellationToken),
            "track-genres" => await BuildTrackGenreAsync(entityId, cancellationToken),
            "track-moods" => await BuildTrackMoodAsync(entityId, cancellationToken),
            "track-instruments" => await BuildTrackInstrumentAsync(entityId, cancellationToken),
            "track-sung-versions" => await BuildTrackSungVersionAsync(entityId, cancellationToken),
            "track-related" => await BuildTrackRelationAsync(entityId, cancellationToken),
            "track-version-types" => await BuildTrackVersionTypeAsync(entityId, cancellationToken),
            _ => null
        };

        return vm is null ? NotFound() : PartialView("_RelatedItemsEditor", vm);
    }

    /// <summary>
    /// POST: Adds an assignment row. Form fields: kind, entityId, optionId, extra, roleId.
    /// </summary>
    [HttpPost]
    [Route("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(CancellationToken cancellationToken = default)
    {
        var kind = Request.Form["kind"].ToString();
        var entityId = int.TryParse(Request.Form["entityId"], out var eId) ? eId : 0;
        var optionId = int.TryParse(Request.Form["optionId"], out var oId) ? oId : 0;
        var roleId = int.TryParse(Request.Form["roleId"], out var rId) ? rId : (int?)null;
        var extra = Request.Form["extra"].ToString().Trim();

        if (entityId == 0)
        {
            return Json(new { success = false, errors = new[] { "Entity id is required." } });
        }

        try
        {
            switch (kind)
            {
                case "album-genres":
                    if (optionId == 0 || await _db.AlbumGenres.AnyAsync(x => x.AlbumId == entityId && x.GenreId == optionId, cancellationToken)) return Duplicate();
                    _db.AlbumGenres.Add(new AlbumGenre { AlbumId = entityId, GenreId = optionId });
                    break;
                case "album-moods":
                    if (optionId == 0 || await _db.AlbumMoods.AnyAsync(x => x.AlbumId == entityId && x.MoodId == optionId, cancellationToken)) return Duplicate();
                    _db.AlbumMoods.Add(new AlbumMood { AlbumId = entityId, MoodId = optionId });
                    break;
                case "album-languages":
                    if (optionId == 0 || await _db.AlbumLanguages.AnyAsync(x => x.AlbumId == entityId && x.LanguageId == optionId, cancellationToken)) return Duplicate();
                    _db.AlbumLanguages.Add(new AlbumLanguage { AlbumId = entityId, LanguageId = optionId });
                    break;
                case "album-countries":
                    if (optionId == 0 || await _db.AlbumCountries.AnyAsync(x => x.AlbumId == entityId && x.CountryId == optionId, cancellationToken)) return Duplicate();
                    _db.AlbumCountries.Add(new AlbumCountry { AlbumId = entityId, CountryId = optionId, CountryRoleTypeId = roleId });
                    break;
                case "album-companies":
                    if (optionId == 0 || await _db.AlbumCompanies.AnyAsync(x => x.AlbumId == entityId && x.CompanyId == optionId, cancellationToken)) return Duplicate();
                    _db.AlbumCompanies.Add(new AlbumCompany
                    {
                        AlbumId = entityId,
                        CompanyId = optionId,
                        CompanyRoleTypeId = roleId,
                        CatalogNumber = string.IsNullOrWhiteSpace(extra) ? null : extra
                    });
                    break;
                case "album-identifiers":
                    if (string.IsNullOrWhiteSpace(extra))
                        return Json(new { success = false, errors = new[] { "Identifier value is required." } });
                    _db.AlbumIdentifiers.Add(new AlbumIdentifier { AlbumId = entityId, IdentifierTypeId = roleId, Value = extra });
                    break;
                case "track-genres":
                    if (optionId == 0 || await _db.TrackGenres.AnyAsync(x => x.TrackId == entityId && x.GenreId == optionId, cancellationToken)) return Duplicate();
                    _db.TrackGenres.Add(new TrackGenre { TrackId = entityId, GenreId = optionId });
                    break;
                case "track-moods":
                    if (optionId == 0 || await _db.TrackMoods.AnyAsync(x => x.TrackId == entityId && x.MoodId == optionId, cancellationToken)) return Duplicate();
                    _db.TrackMoods.Add(new TrackMood { TrackId = entityId, MoodId = optionId });
                    break;
                case "track-instruments":
                    if (optionId == 0 || await _db.TrackInstruments.AnyAsync(x => x.TrackId == entityId && x.InstrumentId == optionId, cancellationToken)) return Duplicate();
                    _db.TrackInstruments.Add(new TrackInstrument { TrackId = entityId, InstrumentId = optionId });
                    break;
                case "track-sung-versions":
                    if (optionId == 0 || await _db.TrackSungVersions.AnyAsync(x => x.TrackId == entityId && x.SungVersionId == optionId, cancellationToken)) return Duplicate();
                    _db.TrackSungVersions.Add(new TrackSungVersion { TrackId = entityId, SungVersionId = optionId, SequenceNumber = 0, IsPrimary = false });
                    break;
                case "track-related":
                    if (optionId == 0 || optionId == entityId || await _db.TrackRelations.AnyAsync(x => x.TrackId == entityId && x.RelatedTrackId == optionId, cancellationToken)) return Duplicate();
                    _db.TrackRelations.Add(new TrackRelation { TrackId = entityId, RelatedTrackId = optionId, TrackRelationTypeId = roleId ?? 0 });
                    break;
                case "track-version-types":
                    if (optionId == 0 || await _db.TrackVersionTypeAssignments.AnyAsync(x => x.TrackId == entityId && x.TrackVersionTypeId == optionId, cancellationToken)) return Duplicate();
                    _db.TrackVersionTypeAssignments.Add(new TrackVersionTypeAssignment { TrackId = entityId, TrackVersionTypeId = optionId });
                    break;
                default:
                    return NotFound();
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Related item added: kind={Kind}, entityId={EntityId}, optionId={OptionId}", kind, entityId, optionId);
            await InvalidateEntityCacheAsync(kind.StartsWith("album", StringComparison.Ordinal) ? AlbumEntityTypeId : TrackEntityTypeId, entityId, "Updated");
            await InvalidateBroadCacheAsync();
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add related item kind={Kind}", kind);
            return Json(new { success = false, errors = new[] { "An error occurred while saving." } });
        }
    }

    /// <summary>
    /// POST: Removes an assignment row. Route: /admin/related-items/remove/{kind}/{id}
    /// </summary>
    [HttpPost]
    [Route("remove/{kind}/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Remove(
        string kind,
        int id,
        CancellationToken cancellationToken = default)
    {
        var entityTypeId = kind.StartsWith("album", StringComparison.Ordinal) ? AlbumEntityTypeId : TrackEntityTypeId;
        var entityId = 0;

        try
        {
            switch (kind)
            {
                case "album-genres":
                    var ag = await _db.AlbumGenres.FirstOrDefaultAsync(x => x.AlbumGenreId == id, cancellationToken);
                    if (ag is not null) { entityId = ag.AlbumId; _db.AlbumGenres.Remove(ag); }
                    break;
                case "album-moods":
                    var am = await _db.AlbumMoods.FirstOrDefaultAsync(x => x.AlbumMoodId == id, cancellationToken);
                    if (am is not null) { entityId = am.AlbumId; _db.AlbumMoods.Remove(am); }
                    break;
                case "album-languages":
                    var al = await _db.AlbumLanguages.FirstOrDefaultAsync(x => x.AlbumLanguageId == id, cancellationToken);
                    if (al is not null) { entityId = al.AlbumId; _db.AlbumLanguages.Remove(al); }
                    break;
                case "album-countries":
                    var ac = await _db.AlbumCountries.FirstOrDefaultAsync(x => x.AlbumCountryId == id, cancellationToken);
                    if (ac is not null) { entityId = ac.AlbumId; _db.AlbumCountries.Remove(ac); }
                    break;
                case "album-companies":
                    var ap = await _db.AlbumCompanies.FirstOrDefaultAsync(x => x.AlbumCompanyId == id, cancellationToken);
                    if (ap is not null) { entityId = ap.AlbumId; _db.AlbumCompanies.Remove(ap); }
                    break;
                case "album-identifiers":
                    var ai = await _db.AlbumIdentifiers.FirstOrDefaultAsync(x => x.AlbumIdentifierId == id, cancellationToken);
                    if (ai is not null) { entityId = ai.AlbumId; _db.AlbumIdentifiers.Remove(ai); }
                    break;
                case "track-genres":
                    var tg = await _db.TrackGenres.FirstOrDefaultAsync(x => x.TrackGenreId == id, cancellationToken);
                    if (tg is not null) { entityId = tg.TrackId; _db.TrackGenres.Remove(tg); }
                    break;
                case "track-moods":
                    var tm = await _db.TrackMoods.FirstOrDefaultAsync(x => x.TrackMoodId == id, cancellationToken);
                    if (tm is not null) { entityId = tm.TrackId; _db.TrackMoods.Remove(tm); }
                    break;
                case "track-instruments":
                    var ti = await _db.TrackInstruments.FirstOrDefaultAsync(x => x.TrackInstrumentId == id, cancellationToken);
                    if (ti is not null) { entityId = ti.TrackId; _db.TrackInstruments.Remove(ti); }
                    break;
                case "track-sung-versions":
                    var tsv = await _db.TrackSungVersions.FirstOrDefaultAsync(x => x.TrackSungVersionId == id, cancellationToken);
                    if (tsv is not null) { entityId = tsv.TrackId; _db.TrackSungVersions.Remove(tsv); }
                    break;
                case "track-related":
                    var tr = await _db.TrackRelations.FirstOrDefaultAsync(x => x.TrackRelationId == id, cancellationToken);
                    if (tr is not null) { entityId = tr.TrackId; _db.TrackRelations.Remove(tr); }
                    break;
                case "track-version-types":
                    var tv = await _db.TrackVersionTypeAssignments.FirstOrDefaultAsync(x => x.TrackVersionTypeAssignmentId == id, cancellationToken);
                    if (tv is not null) { entityId = tv.TrackId; _db.TrackVersionTypeAssignments.Remove(tv); }
                    break;
                default:
                    return NotFound();
            }

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Related item removed: kind={Kind}, id={Id}", kind, id);
            if (entityId > 0)
            {
                await InvalidateEntityCacheAsync(entityTypeId, entityId, "Updated");
                await InvalidateBroadCacheAsync();
            }
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove related item kind={Kind}, id={Id}", kind, id);
            return Json(new { success = false, errors = new[] { "An error occurred while deleting." } });
        }
    }

    // ──────────────────────────────────────────────
    //  Per-kind view model builders
    // ──────────────────────────────────────────────

    private static RelatedItemsEditorViewModel Make(string kind, int entityTypeId, int entityId, string title,
        IReadOnlyList<RelatedItemRow> items, IReadOnlyList<RelatedItemOption> options,
        bool usesDropdown = true, bool hasRoleField = false, string roleFieldLabel = "",
        IReadOnlyList<RelatedItemOption>? roleOptions = null,
        bool hasExtraField = false, string extraFieldLabel = "", string extraFieldPlaceholder = "")
        => new()
        {
            Kind = kind,
            EntityTypeId = entityTypeId,
            EntityId = entityId,
            Title = title,
            Items = items,
            Options = options,
            UsesDropdown = usesDropdown,
            HasRoleField = hasRoleField,
            RoleFieldLabel = roleFieldLabel,
            RoleOptions = roleOptions ?? [],
            HasExtraField = hasExtraField,
            ExtraFieldLabel = extraFieldLabel,
            ExtraFieldPlaceholder = extraFieldPlaceholder
        };

    private async Task<RelatedItemsEditorViewModel> BuildAlbumGenreAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.AlbumGenres.Where(x => x.AlbumId == entityId).Include(x => x.Genre).OrderBy(x => x.Genre.Name).ToListAsync(ct);
        var options = await _db.Genres.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.GenreId, Label = x.Name }).ToListAsync(ct);
        return Make("album-genres", AlbumEntityTypeId, entityId, "Genres",
            rows.Select(x => new RelatedItemRow { Id = x.AlbumGenreId, Label = x.Genre?.Name ?? $"Genre {x.GenreId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildAlbumMoodAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.AlbumMoods.Where(x => x.AlbumId == entityId).Include(x => x.Mood).OrderBy(x => x.Mood.Name).ToListAsync(ct);
        var options = await _db.Moods.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.MoodId, Label = x.Name }).ToListAsync(ct);
        return Make("album-moods", AlbumEntityTypeId, entityId, "Moods",
            rows.Select(x => new RelatedItemRow { Id = x.AlbumMoodId, Label = x.Mood?.Name ?? $"Mood {x.MoodId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildAlbumLanguageAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.AlbumLanguages.Where(x => x.AlbumId == entityId).Include(x => x.Language).OrderBy(x => x.Language!.Name).ToListAsync(ct);
        var options = await _db.Languages.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.LanguageId, Label = x.Name }).ToListAsync(ct);
        return Make("album-languages", AlbumEntityTypeId, entityId, "Languages",
            rows.Select(x => new RelatedItemRow { Id = x.AlbumLanguageId, Label = x.Language?.Name ?? $"Language {x.LanguageId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildAlbumCountryAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.AlbumCountries.Where(x => x.AlbumId == entityId)
            .Include(x => x.Country).Include(x => x.CountryRoleType).OrderBy(x => x.Country!.Name).ToListAsync(ct);
        var options = await _db.Countries.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.CountryId, Label = x.Name }).ToListAsync(ct);
        var roles = await _db.CountryRoleTypes.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.CountryRoleTypeId, Label = x.Name ?? x.Code }).ToListAsync(ct);
        return Make("album-countries", AlbumEntityTypeId, entityId, "Countries",
            rows.Select(x => new RelatedItemRow
            {
                Id = x.AlbumCountryId,
                Label = x.Country?.Name ?? $"Country {x.CountryId}",
                RoleId = x.CountryRoleTypeId,
                RoleLabel = x.CountryRoleType?.Name
            }).ToList(), options, hasRoleField: true, roleFieldLabel: "Role", roleOptions: roles);
    }

    private async Task<RelatedItemsEditorViewModel> BuildAlbumCompanyAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.AlbumCompanies.Where(x => x.AlbumId == entityId)
            .Include(x => x.Company).Include(x => x.CompanyRoleType).OrderBy(x => x.Company.Name).ToListAsync(ct);
        var options = await _db.Companies.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.CompanyId, Label = x.Name }).ToListAsync(ct);
        var roles = await _db.CompanyRoleTypes.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.CompanyRoleTypeId, Label = x.Name ?? x.Code }).ToListAsync(ct);
        return Make("album-companies", AlbumEntityTypeId, entityId, "Companies",
            rows.Select(x => new RelatedItemRow
            {
                Id = x.AlbumCompanyId,
                Label = x.Company?.Name ?? $"Company {x.CompanyId}",
                Extra = x.CatalogNumber,
                RoleId = x.CompanyRoleTypeId,
                RoleLabel = x.CompanyRoleType?.Name
            }).ToList(), options,
            hasRoleField: true, roleFieldLabel: "Role", roleOptions: roles,
            hasExtraField: true, extraFieldLabel: "Catalog number", extraFieldPlaceholder: "e.g. LC 00000");
    }

    private async Task<RelatedItemsEditorViewModel> BuildAlbumIdentifierAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.AlbumIdentifiers.Where(x => x.AlbumId == entityId)
            .Include(x => x.IdentifierType).OrderBy(x => x.IdentifierType!.Name).ToListAsync(ct);
        var roles = await _db.IdentifierTypes.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.IdentifierTypeId, Label = x.Name ?? x.Code }).ToListAsync(ct);
        return Make("album-identifiers", AlbumEntityTypeId, entityId, "Identifiers",
            rows.Select(x => new RelatedItemRow
            {
                Id = x.AlbumIdentifierId,
                Label = $"{x.IdentifierType?.Name ?? "Identifier"}: {x.Value}",
                RoleId = x.IdentifierTypeId,
                RoleLabel = x.IdentifierType?.Name
            }).ToList(), [],
            usesDropdown: false, hasRoleField: true, roleFieldLabel: "Type", roleOptions: roles,
            hasExtraField: true, extraFieldLabel: "Value", extraFieldPlaceholder: "e.g. 0602547093027");
    }

    private async Task<RelatedItemsEditorViewModel> BuildTrackGenreAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.TrackGenres.Where(x => x.TrackId == entityId).Include(x => x.Genre).OrderBy(x => x.Genre.Name).ToListAsync(ct);
        var options = await _db.Genres.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.GenreId, Label = x.Name }).ToListAsync(ct);
        return Make("track-genres", TrackEntityTypeId, entityId, "Genres",
            rows.Select(x => new RelatedItemRow { Id = x.TrackGenreId, Label = x.Genre?.Name ?? $"Genre {x.GenreId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildTrackMoodAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.TrackMoods.Where(x => x.TrackId == entityId).Include(x => x.Mood).OrderBy(x => x.Mood.Name).ToListAsync(ct);
        var options = await _db.Moods.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.MoodId, Label = x.Name }).ToListAsync(ct);
        return Make("track-moods", TrackEntityTypeId, entityId, "Moods",
            rows.Select(x => new RelatedItemRow { Id = x.TrackMoodId, Label = x.Mood?.Name ?? $"Mood {x.MoodId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildTrackInstrumentAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.TrackInstruments.Where(x => x.TrackId == entityId).Include(x => x.Instrument).OrderBy(x => x.Instrument.Name).ToListAsync(ct);
        var options = await _db.Instruments.Where(x => !x.IsDeleted).OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.InstrumentId, Label = x.Name }).ToListAsync(ct);
        return Make("track-instruments", TrackEntityTypeId, entityId, "Instruments",
            rows.Select(x => new RelatedItemRow { Id = x.TrackInstrumentId, Label = x.Instrument?.Name ?? $"Instrument {x.InstrumentId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildTrackSungVersionAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.TrackSungVersions.Where(x => x.TrackId == entityId).Include(x => x.SungVersion).OrderBy(x => x.SungVersion.Title).ToListAsync(ct);
        var options = await _db.SungVersions.Where(x => !x.IsDeleted).OrderBy(x => x.Title).Select(x => new RelatedItemOption { Id = x.SungVersionId, Label = x.Title }).ToListAsync(ct);
        return Make("track-sung-versions", TrackEntityTypeId, entityId, "Sung Versions",
            rows.Select(x => new RelatedItemRow { Id = x.TrackSungVersionId, Label = x.SungVersion?.Title ?? $"Sung version {x.SungVersionId}" }).ToList(), options);
    }

    private async Task<RelatedItemsEditorViewModel> BuildTrackRelationAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.TrackRelations.Where(x => x.TrackId == entityId)
            .Include(x => x.RelatedTrack).Include(x => x.TrackRelationType).OrderBy(x => x.TrackRelationType.Name).ToListAsync(ct);
        var options = await _db.Tracks.Where(x => !x.IsDeleted && x.TrackId != entityId)
            .OrderBy(x => x.Title).Select(x => new RelatedItemOption { Id = x.TrackId, Label = x.Title }).ToListAsync(ct);
        var roles = await _db.TrackRelationTypes.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.TrackRelationTypeId, Label = x.Name ?? x.Code }).ToListAsync(ct);
        return Make("track-related", TrackEntityTypeId, entityId, "Related Tracks",
            rows.Select(x => new RelatedItemRow
            {
                Id = x.TrackRelationId,
                Label = x.RelatedTrack?.Title ?? $"Track {x.RelatedTrackId}",
                RoleId = x.TrackRelationTypeId,
                RoleLabel = x.TrackRelationType?.Name
            }).ToList(), options,
            hasRoleField: true, roleFieldLabel: "Relation type", roleOptions: roles);
    }

    private async Task<RelatedItemsEditorViewModel> BuildTrackVersionTypeAsync(int entityId, CancellationToken ct)
    {
        var rows = await _db.TrackVersionTypeAssignments.Where(x => x.TrackId == entityId)
            .Include(x => x.TrackVersionType).OrderBy(x => x.TrackVersionType.Name).ToListAsync(ct);
        var options = await _db.TrackVersionTypes.OrderBy(x => x.Name).Select(x => new RelatedItemOption { Id = x.TrackVersionTypeId, Label = x.Name }).ToListAsync(ct);
        return Make("track-version-types", TrackEntityTypeId, entityId, "Version Types",
            rows.Select(x => new RelatedItemRow { Id = x.TrackVersionTypeAssignmentId, Label = x.TrackVersionType?.Name ?? $"Version type {x.TrackVersionTypeId}" }).ToList(), options);
    }

    private JsonResult Duplicate()
        => Json(new { success = false, errors = new[] { "This item is already assigned." } });
}
