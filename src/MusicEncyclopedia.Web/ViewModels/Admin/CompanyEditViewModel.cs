using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the company create/edit form.
/// Maps to/from the <see cref="Company"/> entity.
/// </summary>
public sealed class CompanyEditViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(500, ErrorMessage = "Name must not exceed 500 characters.")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "نام مرتب‌سازی")]
    public string? NameSort { get; set; }

    [StringLength(500)]
    [Display(Name = "نام اصلی")]
    public string? OriginalName { get; set; }

    [StringLength(500)]
    [Display(Name = "نام انگلیسی")]
    public string? EnglishName { get; set; }

    [Display(Name = "نوع شرکت")]
    public int? CompanyTypeId { get; set; }

    [Display(Name = "کشور")]
    public int? CountryId { get; set; }

    [StringLength(500)]
    [Display(Name = "وب‌سایت")]
    public string? Website { get; set; }

    [Display(Name = "تاریخچه")]
    public string? History { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int CompanyId { get; set; }

    public IReadOnlyList<CompanyType> CompanyTypes { get; set; } = [];
    public IReadOnlyList<Country> Countries { get; set; } = [];
}
