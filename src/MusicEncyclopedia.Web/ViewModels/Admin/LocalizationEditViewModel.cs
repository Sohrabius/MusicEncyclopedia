using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the localization create/edit form.
/// Maps to/from the <see cref="Localization"/> entity.
/// </summary>
public sealed class LocalizationEditViewModel
{
    public int LocalizationId { get; set; }

    [Required(ErrorMessage = "Entity type is required.")]
    [Display(Name = "نوع موجودیت")]
    public int EntityTypeId { get; set; }

    [Required(ErrorMessage = "Entity ID is required.")]
    [Display(Name = "شناسه موجودیت")]
    public int EntityId { get; set; }

    [Display(Name = "زبان")]
    public int? LanguageId { get; set; }

    [Required(ErrorMessage = "Field name is required.")]
    [StringLength(255, ErrorMessage = "Field name must not exceed 255 characters.")]
    [Display(Name = "نام فیلد")]
    public string FieldName { get; set; } = "";

    [Required(ErrorMessage = "Localized text is required.")]
    [Display(Name = "متن بومی‌شده")]
    public string LocalizedText { get; set; } = "";

    // Dropdown data
    public IReadOnlyList<EntityType> EntityTypes { get; set; } = [];
    public IReadOnlyList<Language> Languages { get; set; } = [];
}
