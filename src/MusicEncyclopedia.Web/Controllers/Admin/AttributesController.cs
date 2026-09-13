using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MusicEncyclopedia.Core.Constants;
using MusicEncyclopedia.Data;
using MusicEncyclopedia.Data.Entities;
using MusicEncyclopedia.Web.ViewModels.Admin;

namespace MusicEncyclopedia.Web.Controllers.Admin;

[Route("/admin/attributes")]
[Authorize(Policy = PermissionConstants.CanManageLookupTables)]
public sealed class AttributesController(AppDbContext db) : AdminBaseController
{
    private static readonly string[] DataTypes = ["STRING", "INTEGER", "DECIMAL", "DATE", "DATETIME", "BOOLEAN"];

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        ViewData["Title"] = "Attribute Definitions";
        ViewData["ActiveMenu"] = "Attributes";
        return View(new AttributeDefinitionListViewModel
        {
            Items = await db.AttributeDefinitions.OrderBy(x => x.EntityTypeId).ThenBy(x => x.Name).ToListAsync(cancellationToken),
            EntityTypes = await db.EntityTypes.OrderBy(x => x.Name).ToListAsync(cancellationToken),
            DataTypes = DataTypes
        });
    }

    [HttpPost("create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AttributeDefinitionFormModel model, CancellationToken cancellationToken = default)
    {
        if (!await IsValidAsync(model, null, cancellationToken))
        {
            SetErrorMessage("تعریف ویژگی نامعتبر یا تکراری است.");
            return RedirectToAction(nameof(Index));
        }

        db.AttributeDefinitions.Add(new AttributeDefinition
        {
            EntityTypeId = model.EntityTypeId,
            Name = model.Name.Trim(),
            DataTypeCode = model.DataTypeCode.Trim().ToUpperInvariant(),
            IsRequired = model.IsRequired
        });
        await db.SaveChangesAsync(cancellationToken);
        SetSuccessMessage("تعریف ویژگی ایجاد شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(AttributeDefinitionFormModel model, CancellationToken cancellationToken = default)
    {
        var item = await db.AttributeDefinitions.FirstOrDefaultAsync(
            x => x.AttributeDefinitionId == model.AttributeDefinitionId, cancellationToken);
        if (item is null || !await IsValidAsync(model, item.AttributeDefinitionId, cancellationToken))
        {
            SetErrorMessage("تعریف ویژگی نامعتبر یا تکراری است.");
            return RedirectToAction(nameof(Index));
        }

        item.EntityTypeId = model.EntityTypeId;
        item.Name = model.Name.Trim();
        item.DataTypeCode = model.DataTypeCode.Trim().ToUpperInvariant();
        item.IsRequired = model.IsRequired;
        await db.SaveChangesAsync(cancellationToken);
        SetSuccessMessage("تعریف ویژگی به‌روزرسانی شد.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = PermissionConstants.CanDeleteContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken = default)
    {
        var item = await db.AttributeDefinitions.FirstOrDefaultAsync(x => x.AttributeDefinitionId == id, cancellationToken);
        if (item is null)
        {
            SetErrorMessage("تعریف ویژگی یافت نشد.");
            return RedirectToAction(nameof(Index));
        }

        if (await db.AttributeValues.AnyAsync(x => x.AttributeDefinitionId == id, cancellationToken))
        {
            SetErrorMessage("این تعریف در حال استفاده است و قابل حذف نیست.");
            return RedirectToAction(nameof(Index));
        }

        db.AttributeDefinitions.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        SetSuccessMessage("تعریف ویژگی حذف شد.");
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> IsValidAsync(AttributeDefinitionFormModel model, int? excludingId, CancellationToken ct)
    {
        var code = model.DataTypeCode?.Trim().ToUpperInvariant();
        return ModelState.IsValid
            && DataTypes.Contains(code, StringComparer.Ordinal)
            && await db.EntityTypes.AnyAsync(x => x.EntityTypeId == model.EntityTypeId, ct)
            && !await db.AttributeDefinitions.AnyAsync(x => x.AttributeDefinitionId != excludingId
                && x.EntityTypeId == model.EntityTypeId && x.Name == model.Name.Trim(), ct);
    }
}
