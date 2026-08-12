using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the citation create/edit form.
/// Maps to/from the <see cref="Citation"/> entity.
/// </summary>
public sealed class CitationEditViewModel
{
    public int CitationId { get; set; }

    [Required(ErrorMessage = "Entity type is required.")]
    [Display(Name = "نوع موجودیت")]
    public int EntityTypeId { get; set; }

    [Required(ErrorMessage = "Entity ID is required.")]
    [Display(Name = "شناسه موجودیت")]
    public int EntityId { get; set; }

    [Display(Name = "منبع")]
    public int? SourceId { get; set; }

    [StringLength(255, ErrorMessage = "Field name must not exceed 255 characters.")]
    [Display(Name = "نام فیلد")]
    public string? FieldName { get; set; }

    [Display(Name = "نقل‌قول")]
    public string? Quote { get; set; }

    [StringLength(50, ErrorMessage = "Page number must not exceed 50 characters.")]
    [Display(Name = "شماره صفحه")]
    public string? PageNumber { get; set; }

    [StringLength(2000, ErrorMessage = "URL must not exceed 2000 characters.")]
    [Display(Name = "URL")]
    public string? Url { get; set; }

    [Display(Name = "تاریخ دسترسی")]
    public DateOnly? AccessedDate { get; set; }

    [Display(Name = "یادداشت‌ها")]
    public string? Notes { get; set; }

    // Dropdown data
    public IReadOnlyList<EntityType> EntityTypes { get; set; } = [];
    public IReadOnlyList<Source> Sources { get; set; } = [];
}
