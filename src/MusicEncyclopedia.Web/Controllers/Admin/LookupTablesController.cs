using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Admin lookup tables controller.
/// Route: /admin/lookup-tables
/// Provides a single, generic CRUD surface over the small Code+Name "type" tables
/// (PublicationType, SourceType, MediaType, PersonKind, ...). All registered tables
/// share the same shape — a [Key] int Id plus Code and Name strings — so the same
/// editor works for every one of them.
/// </summary>
[Route("/admin/lookup-tables")]
[Authorize(Policy = PermissionConstants.CanManageLookupTables)]
public sealed class LookupTablesController : AdminBaseController
{
    private readonly AppDbContext _db;

    public LookupTablesController(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// The lookup tables managed by this section, keyed by route slug.
    /// Tables with dedicated admin sections (Genres, Moods, Instruments) are excluded.
    /// </summary>
    private static readonly (string Key, string Label, Type EntityType, string DbSetName)[] Registry =
    [
        ("album-categories", "Album Categories", typeof(AlbumCategory), nameof(AppDbContext.AlbumCategories)),
        ("album-relation-types", "Album Relation Types", typeof(AlbumRelationType), nameof(AppDbContext.AlbumRelationTypes)),
        ("alias-types", "Alias Types", typeof(AliasType), nameof(AppDbContext.AliasTypes)),
        ("award-result-types", "Award Result Types", typeof(AwardResultType), nameof(AppDbContext.AwardResultTypes)),
        ("company-role-types", "Company Role Types", typeof(CompanyRoleType), nameof(AppDbContext.CompanyRoleTypes)),
        ("company-types", "Company Types", typeof(CompanyType), nameof(AppDbContext.CompanyTypes)),
        ("countries", "Countries", typeof(Country), nameof(AppDbContext.Countries)),
        ("country-role-types", "Country Role Types", typeof(CountryRoleType), nameof(AppDbContext.CountryRoleTypes)),
        ("event-types", "Event Types", typeof(EventType), nameof(AppDbContext.EventTypes)),
        ("identifier-types", "Identifier Types", typeof(IdentifierType), nameof(AppDbContext.IdentifierTypes)),
        ("instrument-families", "Instrument Families", typeof(InstrumentFamily), nameof(AppDbContext.InstrumentFamilies)),
        ("languages", "Languages", typeof(Language), nameof(AppDbContext.Languages)),
        ("link-types", "Link Types", typeof(LinkType), nameof(AppDbContext.LinkTypes)),
        ("location-types", "Location Types", typeof(LocationType), nameof(AppDbContext.LocationTypes)),
        ("lyrics-availability-types", "Lyrics Availability Types", typeof(LyricsAvailabilityType), nameof(AppDbContext.LyricsAvailabilityTypes)),
        ("media-role-types", "Media Role Types", typeof(MediaRoleType), nameof(AppDbContext.MediaRoleTypes)),
        ("media-types", "Media Types", typeof(MediaType), nameof(AppDbContext.MediaTypes)),
        ("musical-keys", "Musical Keys", typeof(MusicalKey), nameof(AppDbContext.MusicalKeys)),
        ("person-kinds", "Person Kinds", typeof(PersonKind), nameof(AppDbContext.PersonKinds)),
        ("publication-types", "Publication Types", typeof(PublicationType), nameof(AppDbContext.PublicationTypes)),
        ("role-scope-types", "Role Scope Types", typeof(RoleScopeType), nameof(AppDbContext.RoleScopeTypes)),
        ("session-types", "Session Types", typeof(SessionType), nameof(AppDbContext.SessionTypes)),
        ("source-types", "Source Types", typeof(SourceType), nameof(AppDbContext.SourceTypes)),
        ("track-relation-types", "Track Relation Types", typeof(TrackRelationType), nameof(AppDbContext.TrackRelationTypes)),
        ("track-version-types", "Track Version Types", typeof(TrackVersionType), nameof(AppDbContext.TrackVersionTypes)),
        ("vocal-styles", "Vocal Styles", typeof(VocalStyle), nameof(AppDbContext.VocalStyles))
    ];

    [HttpGet]
    [Route("")]
    [Route("Index")]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var tables = new List<LookupTableSummary>();
        foreach (var (key, label, entityType, dbSetName) in Registry)
        {
            var query = GetQueryable(dbSetName);
            var count = await ((IQueryable<object>)query).CountAsync(cancellationToken);
            tables.Add(new LookupTableSummary
            {
                Key = key,
                Label = label,
                EntityName = entityType.Name,
                RowCount = count
            });
        }

        ViewData["Title"] = "Lookup Tables";
        ViewData["ActiveMenu"] = "Lookup Tables";

        return View(new LookupTableListViewModel { Tables = tables });
    }

    [HttpGet]
    [Route("{key}")]
    public async Task<IActionResult> Items(string key, CancellationToken cancellationToken = default)
    {
        var (label, entityType, dbSetName) = Resolve(key);
        if (label is null)
        {
            SetErrorMessage("جدول مرجع ناشناخته است.");
            return RedirectToAction(nameof(Index));
        }

        var query = GetQueryable(dbSetName);
        var rows = new List<LookupItemRow>();
        foreach (var entity in await ((IQueryable<object>)query).OrderBy(e => EF.Property<string>(e, "Name")).ToListAsync(cancellationToken))
        {
            rows.Add(new LookupItemRow
            {
                Id = (int)ReadProperty(entity, GetKeyProperty(entityType).Name)!,
                Code = ReadProperty(entity, "Code") as string,
                Name = ReadProperty(entity, "Name") as string
            });
        }

        ViewData["Title"] = $"{label}";
        ViewData["ActiveMenu"] = "Lookup Tables";

        return View(new LookupTableItemsViewModel
        {
            Key = key,
            Label = label,
            EntityName = entityType.Name,
            Items = rows
        });
    }

    [HttpPost]
    [Route("{key}/create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(string key, LookupItemFormModel form, CancellationToken cancellationToken = default)
    {
        var (label, entityType, dbSetName) = Resolve(key);
        if (label is null)
        {
            SetErrorMessage("جدول مرجع ناشناخته است.");
            return RedirectToAction(nameof(Index));
        }

        if (!ModelState.IsValid)
        {
            SetErrorMessage("لطفاً فیلدهای الزامی را پر کنید.");
            return RedirectToAction(nameof(Items), new { key });
        }

        var entity = Activator.CreateInstance(entityType)!;
        WriteProperty(entity, "Code", form.Code?.Trim());
        WriteProperty(entity, "Name", form.Name?.Trim());

        _db.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        SetSuccessMessage($"«{form.Name}» به {label}.");
        return RedirectToAction(nameof(Items), new { key });
    }

    [HttpPost]
    [Route("{key}/update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(string key, LookupItemFormModel form, CancellationToken cancellationToken = default)
    {
        var (label, entityType, dbSetName) = Resolve(key);
        if (label is null)
        {
            SetErrorMessage("جدول مرجع ناشناخته است.");
            return RedirectToAction(nameof(Index));
        }

        var query = GetQueryable(dbSetName);
        var keyProperty = GetKeyProperty(entityType);
        var entity = await ((IQueryable<object>)query)
            .FirstOrDefaultAsync(e => EF.Property<int>(e, keyProperty.Name) == form.Id, cancellationToken);

        if (entity is null)
        {
            SetErrorMessage("مورد دیگر وجود ندارد.");
            return RedirectToAction(nameof(Items), new { key });
        }

        WriteProperty(entity, "Code", form.Code?.Trim());
        WriteProperty(entity, "Name", form.Name?.Trim());

        await _db.SaveChangesAsync(cancellationToken);

        SetSuccessMessage($"«{form.Name}» در {label}.");
        return RedirectToAction(nameof(Items), new { key });
    }

    [HttpPost]
    [Route("{key}/delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(string key, LookupItemFormModel form, CancellationToken cancellationToken = default)
    {
        var (label, entityType, dbSetName) = Resolve(key);
        if (label is null)
        {
            SetErrorMessage("جدول مرجع ناشناخته است.");
            return RedirectToAction(nameof(Index));
        }

        var query = GetQueryable(dbSetName);
        var keyProperty = GetKeyProperty(entityType);
        var entity = await ((IQueryable<object>)query)
            .FirstOrDefaultAsync(e => EF.Property<int>(e, keyProperty.Name) == form.Id, cancellationToken);

        if (entity is null)
        {
            SetErrorMessage("مورد دیگر وجود ندارد.");
            return RedirectToAction(nameof(Items), new { key });
        }

        var name = ReadProperty(entity, "Name") as string ?? ReadProperty(entity, "Code") as string ?? $"#{form.Id}";

        try
        {
            _db.Remove(entity);
            await _db.SaveChangesAsync(cancellationToken);
            SetSuccessMessage($"«{name}» از {label}.");
        }
        catch (DbUpdateException)
        {
            // The row is referenced elsewhere — do not cascade or corrupt data.
            SetErrorMessage($"«{name}» در حال استفاده است و قابل حذف نیست.");
        }

        return RedirectToAction(nameof(Items), new { key });
    }

    private (string? Label, Type EntityType, string DbSetName) Resolve(string key)
    {
        var match = Registry.FirstOrDefault(r => string.Equals(r.Key, key, StringComparison.OrdinalIgnoreCase));
        return match == default
            ? (null, null!, string.Empty)
            : (match.Label, match.EntityType, match.DbSetName);
    }

    private IQueryable GetQueryable(string dbSetName)
    {
        var property = typeof(AppDbContext).GetProperty(dbSetName)!;
        return (IQueryable)property.GetValue(_db)!;
    }

    private static PropertyInfo GetKeyProperty(Type entityType)
    {
        return entityType.GetProperties().First(p =>
            p.GetCustomAttribute<KeyAttribute>() is not null ||
            string.Equals(p.Name, $"{entityType.Name}Id", StringComparison.Ordinal) ||
            string.Equals(p.Name, "Id", StringComparison.Ordinal));
    }

    private static object? ReadProperty(object entity, string name)
    {
        return entity.GetType().GetProperty(name)?.GetValue(entity);
    }

    private static void WriteProperty(object entity, string name, string? value)
    {
        var property = entity.GetType().GetProperty(name);
        if (property is not null && property.CanWrite)
        {
            property.SetValue(entity, value);
        }
    }
}
