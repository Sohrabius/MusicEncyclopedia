using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the album create/edit form (spec 10.5).
/// Maps to/from the <see cref="Album"/> entity.
/// </summary>
public sealed class AlbumEditViewModel
{
    // ──────────────────────────────────────────────
    // Core Fields
    // ──────────────────────────────────────────────

    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "Sort Title")]
    public string? TitleSort { get; set; }

    [StringLength(500)]
    [Display(Name = "Original Title")]
    public string? OriginalTitle { get; set; }

    [StringLength(500)]
    [Display(Name = "English Title")]
    public string? EnglishTitle { get; set; }

    [Required(ErrorMessage = "Album category is required.")]
    [Display(Name = "Category")]
    public int AlbumCategoryId { get; set; }

    [Display(Name = "Release Date")]
    public DateOnly? ReleaseDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Release Date Precision")]
    public string? ReleaseDatePrecision { get; set; }

    [Display(Name = "Recording Start Date")]
    public DateOnly? RecordingStartDate { get; set; }

    [Display(Name = "Recording End Date")]
    public DateOnly? RecordingEndDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Recording Date Precision")]
    public string? RecordingDatePrecision { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Cover Media")]
    public int? CoverMediaId { get; set; }

    [Display(Name = "Duration (seconds)")]
    public int? DurationSeconds { get; set; }

    [StringLength(1000)]
    [Display(Name = "Copyright Notice")]
    public string? CopyrightNotice { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    [Display(Name = "Is Official")]
    public bool IsOfficial { get; set; }

    // ──────────────────────────────────────────────
    // Concurrency / Audit (set by controller)
    // ──────────────────────────────────────────────

    /// <summary>RowVersion for concurrency checking (Edit only).</summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>Album ID (0 for create, >0 for edit).</summary>
    public int AlbumId { get; set; }

    // ──────────────────────────────────────────────
    // Dropdown Data
    // ──────────────────────────────────────────────

    /// <summary>Available album categories for the dropdown.</summary>
    public IReadOnlyList<AlbumCategory> Categories { get; set; } = [];
}

/// <summary>
/// Lightweight model for populating a select dropdown.
/// </summary>
public sealed class SelectListItemVm
{
    public int Value { get; init; }
    public string Text { get; init; } = "";
}
