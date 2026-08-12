using System.ComponentModel.DataAnnotations;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the mood create/edit form.
/// Maps to/from the <see cref="MusicEncyclopedia.Data.Entities.Mood"/> entity.
/// </summary>
public sealed class MoodEditViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int MoodId { get; set; }
}
