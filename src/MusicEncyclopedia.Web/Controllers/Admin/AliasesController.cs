using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Alias editor controller (spec 10.13).
/// Provides AJAX-based inline alias management for any entity type.
/// Route: /admin/aliases
/// </summary>
[Route("/admin/aliases")]
[Authorize(Policy = PermissionConstants.CanManageTracks)]
public sealed class AliasesController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<AliasesController> _logger;

    public AliasesController(AppDbContext db, ILogger<AliasesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET: Returns the alias editor partial view for a given entity.
    /// </summary>
    [HttpGet]
    [Route("{entityTypeId:int}/{entityId:int}")]
    public async Task<IActionResult> Index(
        int entityTypeId,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        var aliases = await _db.Aliases
            .Where(a => a.EntityTypeId == entityTypeId && a.EntityId == entityId)
            .OrderByDescending(a => a.IsPrimary)
            .ThenBy(a => a.AliasName)
            .ToListAsync(cancellationToken);

        var viewModel = new AliasListViewModel
        {
            EntityTypeId = entityTypeId,
            EntityId = entityId,
            Aliases = aliases.Select(a => new AliasRowViewModel
            {
                AliasId = a.AliasId,
                AliasTypeId = a.AliasTypeId,
                LanguageId = a.LanguageId,
                AliasName = a.AliasName,
                IsPrimary = a.IsPrimary,
                Notes = a.Notes
            }).ToList(),
            AliasTypes = await _db.AliasTypes
                .OrderBy(at => at.Name)
                .ToListAsync(cancellationToken),
            Languages = await _db.Languages
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken)
        };

        return PartialView("_AliasEditor", viewModel);
    }

    /// <summary>
    /// POST: Saves a single alias row (create or update).
    /// </summary>
    [HttpPost]
    [Route("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromForm] AliasRowViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(model.AliasName))
        {
            return Json(new { success = false, errors = new[] { "نام مستعار الزامی است." } });
        }

        if (model.AliasId > 0)
        {
            var alias = await _db.Aliases
                .FirstOrDefaultAsync(a => a.AliasId == model.AliasId, cancellationToken);

            if (alias is null)
            {
                return Json(new { success = false, errors = new[] { "نام مستعار یافت نشد." } });
            }

            alias.AliasTypeId = model.AliasTypeId;
            alias.LanguageId = model.LanguageId;
            alias.AliasName = model.AliasName;
            alias.IsPrimary = model.IsPrimary;
            alias.Notes = model.Notes;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Alias updated: AliasId={AliasId}", alias.AliasId);

            await InvalidateBroadCacheAsync();

            return Json(new { success = true, aliasId = alias.AliasId });
        }
        else
        {
            var entityTypeId = int.TryParse(Request.Form["entityTypeId"], out var etId) ? etId : 0;
            var entityId = int.TryParse(Request.Form["entityId"], out var eId) ? eId : 0;

            if (entityTypeId == 0 || entityId == 0)
            {
                return Json(new { success = false, errors = new[] { "موجودیت type and entity ID are required." } });
            }

            var alias = new Alias
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                AliasTypeId = model.AliasTypeId,
                LanguageId = model.LanguageId,
                AliasName = model.AliasName,
                IsPrimary = model.IsPrimary,
                Notes = model.Notes
            };
            _db.Aliases.Add(alias);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Alias created: AliasId={AliasId}", alias.AliasId);

            await InvalidateBroadCacheAsync();

            return Json(new { success = true, aliasId = alias.AliasId });
        }
    }

    /// <summary>
    /// POST: Deletes an alias row.
    /// </summary>
    [HttpPost]
    [Route("delete/{aliasId:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int aliasId,
        CancellationToken cancellationToken = default)
    {
        var alias = await _db.Aliases
            .FirstOrDefaultAsync(a => a.AliasId == aliasId, cancellationToken);

        if (alias is null)
        {
            return Json(new { success = false, errors = new[] { "نام مستعار یافت نشد." } });
        }

        _db.Aliases.Remove(alias);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Alias deleted: AliasId={AliasId}", aliasId);

        if (alias is not null)
        {
            await InvalidateEntityCacheAsync(alias.EntityTypeId, alias.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        return Json(new { success = true });
    }
}
