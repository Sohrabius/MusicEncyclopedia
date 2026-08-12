using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the person create/edit form (spec 10.8).
/// Maps to/from the <see cref="Person"/> entity.
/// </summary>
public sealed class PersonEditViewModel
{
    // ──────────────────────────────────────────────
    // Core Fields
    // ──────────────────────────────────────────────

    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(500, ErrorMessage = "Full name must not exceed 500 characters.")]
    [Display(Name = "نام کامل")]
    public string FullName { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "نام مرتب‌سازی")]
    public string? FullNameSort { get; set; }

    [StringLength(500)]
    [Display(Name = "نام اصلی")]
    public string? OriginalName { get; set; }

    [StringLength(500)]
    [Display(Name = "نام انگلیسی")]
    public string? EnglishName { get; set; }

    [Display(Name = "نوع شخص")]
    public int? PersonKindId { get; set; }

    [Display(Name = "زندگی‌نامه")]
    public string? Biography { get; set; }

    [Display(Name = "تاریخ تولد")]
    public DateOnly? BirthDate { get; set; }

    [StringLength(50)]
    [Display(Name = "دقت تاریخ تولد")]
    public string? BirthDatePrecision { get; set; }

    [Display(Name = "محل تولد")]
    public int? BirthLocationId { get; set; }

    [Display(Name = "تاریخ فوت")]
    public DateOnly? DeathDate { get; set; }

    [StringLength(50)]
    [Display(Name = "دقت تاریخ فوت")]
    public string? DeathDatePrecision { get; set; }

    [Display(Name = "محل فوت")]
    public int? DeathLocationId { get; set; }

    [Display(Name = "کشور تابعیت")]
    public int? NationalityCountryId { get; set; }

    [Display(Name = "رسانه تصویر")]
    public int? ImageMediaId { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    // ──────────────────────────────────────────────
    // Concurrency / Audit
    // ──────────────────────────────────────────────

    public byte[]? RowVersion { get; set; }
    public int PersonId { get; set; }

    // ──────────────────────────────────────────────
    // Dropdown Data
    // ──────────────────────────────────────────────

    public IReadOnlyList<PersonKind> PersonKinds { get; set; } = [];
    public IReadOnlyList<Country> Countries { get; set; } = [];
}
