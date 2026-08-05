using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Services.Validators;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Credit editor controller (spec 10.7).
/// Provides AJAX-based inline credit management for any entity type.
/// Route: /admin/credits
/// Requires the CanManageTracks permission policy (broad enough for credits).
/// </summary>
[Route("/admin/credits")]
[Authorize(Policy = PermissionConstants.CanManageTracks)]
public sealed class CreditsController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<CreditsController> _logger;

    public CreditsController(AppDbContext db, ILogger<CreditsController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET: Returns the credit editor partial view for a given entity.
    /// </summary>
    [HttpGet]
    [Route("{entityTypeId:int}/{entityId:int}")]
    public async Task<IActionResult> Index(
        int entityTypeId,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        var credits = await _db.Credits
            .Where(c => c.EntityTypeId == entityTypeId && c.EntityId == entityId)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(cancellationToken);

        var viewModel = new CreditListViewModel
        {
            EntityTypeId = entityTypeId,
            EntityId = entityId,
            Credits = credits.Select(c => new CreditRowViewModel
            {
                CreditId = c.CreditId,
                CreditRoleId = c.CreditRoleId,
                RoleScopeTypeId = c.RoleScopeTypeId,
                PersonId = c.PersonId,
                CompanyId = c.CompanyId,
                InstrumentId = c.InstrumentId,
                DisplayOrder = c.DisplayOrder,
                IsPrimary = c.IsPrimary,
                Notes = c.Notes
            }).ToList(),
            CreditRoles = await _db.CreditRoles
                .OrderBy(cr => cr.DisplayOrder)
                .ToListAsync(cancellationToken),
            RoleScopeTypes = await _db.RoleScopeTypes
                .OrderBy(r => r.Name)
                .ToListAsync(cancellationToken),
            People = await _db.People
                .OrderBy(p => p.FullName)
                .ToListAsync(cancellationToken),
            Companies = await _db.Companies
                .OrderBy(c => c.Name)
                .ToListAsync(cancellationToken),
            Instruments = await _db.Instruments
                .OrderBy(i => i.Name)
                .ToListAsync(cancellationToken)
        };

        return PartialView("_CreditEditor", viewModel);
    }

    /// <summary>
    /// POST: Saves a single credit row (create or update).
    /// </summary>
    [HttpPost]
    [Route("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromForm] CreditRowViewModel model,
        CancellationToken cancellationToken = default)
    {
        // Validate with FluentValidation
        var validator = new CreditValidator();
        var validationResult = await validator.ValidateAsync(
            new CreditFormModel
            {
                PersonId = model.PersonId,
                CompanyId = model.CompanyId,
                InstrumentId = model.InstrumentId,
                DisplayOrder = model.DisplayOrder
            },
            cancellationToken);

        if (!validationResult.IsValid)
        {
            return Json(new { success = false, errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        if (model.CreditId > 0)
        {
            // Update existing credit
            var credit = await _db.Credits
                .FirstOrDefaultAsync(c => c.CreditId == model.CreditId, cancellationToken);

            if (credit is null)
            {
                return Json(new { success = false, errors = new[] { "Credit not found." } });
            }

            credit.CreditRoleId = model.CreditRoleId;
            credit.RoleScopeTypeId = model.RoleScopeTypeId;
            credit.PersonId = model.PersonId;
            credit.CompanyId = model.CompanyId;
            credit.InstrumentId = model.InstrumentId;
            credit.DisplayOrder = model.DisplayOrder;
            credit.IsPrimary = model.IsPrimary;
            credit.Notes = model.Notes;
            credit.ModifiedBy = User.Identity?.Name ?? "system";
            credit.ModifiedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Credit updated: CreditId={CreditId}", credit.CreditId);

            // The credit's parent entity and entity type are from the original loaded credit
            await InvalidateBroadCacheAsync();

            return Json(new { success = true, creditId = credit.CreditId });
        }
        else
        {
            // Create new credit — we need EntityTypeId and EntityId from the form
            // These are passed as additional form fields
            var entityTypeId = int.TryParse(Request.Form["entityTypeId"], out var etId) ? etId : 0;
            var entityId = int.TryParse(Request.Form["entityId"], out var eId) ? eId : 0;

            if (entityTypeId == 0 || entityId == 0)
            {
                return Json(new { success = false, errors = new[] { "Entity type and entity ID are required." } });
            }

            var credit = new Credit
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                CreditRoleId = model.CreditRoleId,
                RoleScopeTypeId = model.RoleScopeTypeId,
                PersonId = model.PersonId,
                CompanyId = model.CompanyId,
                InstrumentId = model.InstrumentId,
                DisplayOrder = model.DisplayOrder,
                IsPrimary = model.IsPrimary,
                Notes = model.Notes,
                CreatedBy = User.Identity?.Name ?? "system",
                CreatedAt = DateTime.UtcNow
            };
            _db.Credits.Add(credit);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Credit created: CreditId={CreditId}", credit.CreditId);

            await InvalidateEntityCacheAsync(credit.EntityTypeId, credit.EntityId, "Updated");
            await InvalidateBroadCacheAsync();

            return Json(new { success = true, creditId = credit.CreditId });
        }
    }

    /// <summary>
    /// POST: Deletes a credit row.
    /// </summary>
    [HttpPost]
    [Route("delete/{creditId:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int creditId,
        CancellationToken cancellationToken = default)
    {
        var credit = await _db.Credits
            .FirstOrDefaultAsync(c => c.CreditId == creditId, cancellationToken);

        if (credit is null)
        {
            return Json(new { success = false, errors = new[] { "Credit not found." } });
        }

        _db.Credits.Remove(credit);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Credit deleted: CreditId={CreditId}", creditId);

        if (credit is not null)
        {
            await InvalidateEntityCacheAsync(credit.EntityTypeId, credit.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        return Json(new { success = true });
    }

    /// <summary>
    /// GET: Returns credit roles filtered by entity type for dynamic dropdowns.
    /// </summary>
    [HttpGet]
    [Route("roles/{entityTypeId:int}")]
    public async Task<IActionResult> GetRolesByEntityType(
        int entityTypeId,
        CancellationToken cancellationToken = default)
    {
        var roles = await _db.CreditRoleEntityTypes
            .Where(cr => cr.EntityTypeId == entityTypeId)
            .Include(cr => cr.CreditRole)
            .Select(cr => new { cr.CreditRoleId, cr.CreditRole.Name })
            .OrderBy(cr => cr.Name)
            .ToListAsync(cancellationToken);

        return Json(roles);
    }
}
