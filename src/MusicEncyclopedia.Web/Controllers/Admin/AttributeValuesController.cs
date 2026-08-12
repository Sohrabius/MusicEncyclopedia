using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

/// <summary>
/// Attribute value editor controller (spec 10.15).
/// Provides AJAX-based inline attribute value management for any entity type.
/// Route: /admin/attribute-values
/// </summary>
[Route("/admin/attribute-values")]
[Authorize(Policy = PermissionConstants.CanManageTracks)]
public sealed class AttributeValuesController : AdminBaseController
{
    private readonly AppDbContext _db;
    private readonly ILogger<AttributeValuesController> _logger;

    public AttributeValuesController(AppDbContext db, ILogger<AttributeValuesController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET: Returns the attribute value editor partial view for a given entity.
    /// </summary>
    [HttpGet]
    [Route("{entityTypeId:int}/{entityId:int}")]
    public async Task<IActionResult> Index(
        int entityTypeId,
        int entityId,
        CancellationToken cancellationToken = default)
    {
        var attributeValues = await _db.AttributeValues
            .Where(av => av.EntityTypeId == entityTypeId && av.EntityId == entityId)
            .Include(av => av.AttributeDefinition)
            .OrderBy(av => av.AttributeDefinition.Name)
            .ToListAsync(cancellationToken);

        var viewModel = new AttributeValueListViewModel
        {
            EntityTypeId = entityTypeId,
            EntityId = entityId,
            AttributeValues = attributeValues.Select(av => new AttributeValueRowViewModel
            {
                AttributeValueId = av.AttributeValueId,
                AttributeDefinitionId = av.AttributeDefinitionId,
                LanguageId = av.LanguageId,
                ValueString = av.ValueString,
                ValueInt = av.ValueInt,
                ValueDecimal = av.ValueDecimal,
                ValueDate = av.ValueDate,
                ValueDateTime = av.ValueDateTime,
                ValueBit = av.ValueBit,
                Notes = av.Notes
            }).ToList(),
            AttributeDefinitions = await _db.AttributeDefinitions
                .Where(ad => ad.EntityTypeId == entityTypeId)
                .OrderBy(ad => ad.Name)
                .ToListAsync(cancellationToken),
            Languages = await _db.Languages
                .OrderBy(l => l.Name)
                .ToListAsync(cancellationToken)
        };

        return PartialView("_AttributeValueEditor", viewModel);
    }

    /// <summary>
    /// POST: Saves a single attribute value row (create or update).
    /// </summary>
    [HttpPost]
    [Route("save")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        [FromForm] AttributeValueRowViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (model.AttributeDefinitionId == 0)
        {
            return Json(new { success = false, errors = new[] { "تعریف ویژگی الزامی است." } });
        }

        if (model.AttributeValueId > 0)
        {
            var attributeValue = await _db.AttributeValues
                .FirstOrDefaultAsync(av => av.AttributeValueId == model.AttributeValueId, cancellationToken);

            if (attributeValue is null)
            {
                return Json(new { success = false, errors = new[] { "مقدار ویژگی یافت نشد." } });
            }

            attributeValue.AttributeDefinitionId = model.AttributeDefinitionId;
            attributeValue.LanguageId = model.LanguageId;
            attributeValue.ValueString = model.ValueString;
            attributeValue.ValueInt = model.ValueInt;
            attributeValue.ValueDecimal = model.ValueDecimal;
            attributeValue.ValueDate = model.ValueDate;
            attributeValue.ValueDateTime = model.ValueDateTime;
            attributeValue.ValueBit = model.ValueBit;
            attributeValue.Notes = model.Notes;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("AttributeValue updated: AttributeValueId={AttributeValueId}", attributeValue.AttributeValueId);

            // Invalidate the parent entity's cache
            if (attributeValue is not null)
            {
                await InvalidateEntityCacheAsync(attributeValue.EntityTypeId, attributeValue.EntityId, "Updated");
            }
            await InvalidateBroadCacheAsync();

            return Json(new { success = true, attributeValueId = attributeValue.AttributeValueId });
        }
        else
        {
            var entityTypeId = int.TryParse(Request.Form["entityTypeId"], out var etId) ? etId : 0;
            var entityId = int.TryParse(Request.Form["entityId"], out var eId) ? eId : 0;

            if (entityTypeId == 0 || entityId == 0)
            {
                return Json(new { success = false, errors = new[] { "موجودیت type and entity ID are required." } });
            }

            var attributeValue = new AttributeValue
            {
                EntityTypeId = entityTypeId,
                EntityId = entityId,
                AttributeDefinitionId = model.AttributeDefinitionId,
                LanguageId = model.LanguageId,
                ValueString = model.ValueString,
                ValueInt = model.ValueInt,
                ValueDecimal = model.ValueDecimal,
                ValueDate = model.ValueDate,
                ValueDateTime = model.ValueDateTime,
                ValueBit = model.ValueBit,
                Notes = model.Notes
            };
            _db.AttributeValues.Add(attributeValue);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("AttributeValue created: AttributeValueId={AttributeValueId}", attributeValue.AttributeValueId);

            await InvalidateEntityCacheAsync(attributeValue.EntityTypeId, attributeValue.EntityId, "Updated");
            await InvalidateBroadCacheAsync();

            return Json(new { success = true, attributeValueId = attributeValue.AttributeValueId });
        }
    }

    /// <summary>
    /// POST: Deletes an attribute value row.
    /// </summary>
    [HttpPost]
    [Route("delete/{attributeValueId:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(
        int attributeValueId,
        CancellationToken cancellationToken = default)
    {
        var attributeValue = await _db.AttributeValues
            .FirstOrDefaultAsync(av => av.AttributeValueId == attributeValueId, cancellationToken);

        if (attributeValue is null)
        {
            return Json(new { success = false, errors = new[] { "مقدار ویژگی یافت نشد." } });
        }

        _db.AttributeValues.Remove(attributeValue);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("AttributeValue deleted: AttributeValueId={AttributeValueId}", attributeValueId);

        if (attributeValue is not null)
        {
            await InvalidateEntityCacheAsync(attributeValue.EntityTypeId, attributeValue.EntityId, "Updated");
        }
        await InvalidateBroadCacheAsync();

        return Json(new { success = true });
    }
}
