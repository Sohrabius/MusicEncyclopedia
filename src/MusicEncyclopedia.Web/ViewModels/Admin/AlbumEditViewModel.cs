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
    [Display(Name = "عنوان")]
    public string Title { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "عنوان مرتب‌سازی")]
    public string? TitleSort { get; set; }

    [StringLength(500)]
    [Display(Name = "عنوان اصلی")]
    public string? OriginalTitle { get; set; }

    [StringLength(500)]
    [Display(Name = "عنوان انگلیسی")]
    public string? EnglishTitle { get; set; }

    [Required(ErrorMessage = "Album category is required.")]
    [Display(Name = "دسته‌بندی")]
    public int AlbumCategoryId { get; set; }

    [Display(Name = "تاریخ انتشار")]
    public DateOnly? ReleaseDate { get; set; }

    [StringLength(50)]
    [Display(Name = "دقت تاریخ انتشار")]
    public string? ReleaseDatePrecision { get; set; }

    [Display(Name = "تاریخ شروع ضبط")]
    public DateOnly? RecordingStartDate { get; set; }

    [Display(Name = "تاریخ پایان ضبط")]
    public DateOnly? RecordingEndDate { get; set; }

    [StringLength(50)]
    [Display(Name = "دقت تاریخ ضبط")]
    public string? RecordingDatePrecision { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Display(Name = "رسانه جلد")]
    public int? CoverMediaId { get; set; }

    [Display(Name = "مدت (ثانیه)")]
    public int? DurationSeconds { get; set; }

    [StringLength(1000)]
    [Display(Name = "اعلامیه حق نشر")]
    public string? CopyrightNotice { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    [Display(Name = "رسمی است")]
    public bool IsOfficial { get; set; }

    // ──────────────────────────────────────────────
    // Concurrency / Audit (set by controller)
    // ──────────────────────────────────────────────

    /// <summary>RowVersion for concurrency checking (Edit only).</summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>Album ID (0 for create, >0 for edit).</summary>
    public int AlbumId { get; set; }

    /// <summary>Global entity ID used by polymorphic editors.</summary>
    public int EntityId { get; set; }

    /// <summary>The polymorphic EntityType id for albums (used by related-editor tabs).</summary>
    public int EntityTypeId => 1;

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
