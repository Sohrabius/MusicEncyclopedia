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
    [Display(Name = "Entity Type")]
    public int EntityTypeId { get; set; }

    [Required(ErrorMessage = "Entity ID is required.")]
    [Display(Name = "Entity ID")]
    public int EntityId { get; set; }

    [Display(Name = "Language")]
    public int? LanguageId { get; set; }

    [Required(ErrorMessage = "Field name is required.")]
    [StringLength(255, ErrorMessage = "Field name must not exceed 255 characters.")]
    [Display(Name = "Field Name")]
    public string FieldName { get; set; } = "";

    [Required(ErrorMessage = "Localized text is required.")]
    [Display(Name = "Localized Text")]
    public string LocalizedText { get; set; } = "";

    // Dropdown data
    public IReadOnlyList<EntityType> EntityTypes { get; set; } = [];
    public IReadOnlyList<Language> Languages { get; set; } = [];
}
