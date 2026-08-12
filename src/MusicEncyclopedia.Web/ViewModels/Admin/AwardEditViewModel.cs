using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class AwardEditViewModel
{
    public int AwardId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "سازمان")]
    public string? Organization { get; set; }

    [Display(Name = "کشور")]
    public int? CountryId { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }

    // Dropdown data
    public IReadOnlyList<Country> Countries { get; set; } = [];
}
