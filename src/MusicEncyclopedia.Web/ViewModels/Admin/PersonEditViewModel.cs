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
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "Sort Name")]
    public string? FullNameSort { get; set; }

    [StringLength(500)]
    [Display(Name = "Original Name")]
    public string? OriginalName { get; set; }

    [StringLength(500)]
    [Display(Name = "English Name")]
    public string? EnglishName { get; set; }

    [Display(Name = "Person Kind")]
    public int? PersonKindId { get; set; }

    [Display(Name = "Biography")]
    public string? Biography { get; set; }

    [Display(Name = "Birth Date")]
    public DateOnly? BirthDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Birth Date Precision")]
    public string? BirthDatePrecision { get; set; }

    [Display(Name = "Birth Location")]
    public int? BirthLocationId { get; set; }

    [Display(Name = "Death Date")]
    public DateOnly? DeathDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Death Date Precision")]
    public string? DeathDatePrecision { get; set; }

    [Display(Name = "Death Location")]
    public int? DeathLocationId { get; set; }

    [Display(Name = "Nationality Country")]
    public int? NationalityCountryId { get; set; }

    [Display(Name = "Image Media")]
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
