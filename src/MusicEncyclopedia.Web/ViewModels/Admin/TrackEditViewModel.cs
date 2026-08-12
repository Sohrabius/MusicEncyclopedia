using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the track create/edit form (spec 10.6).
/// Maps to/from the <see cref="Track"/> entity.
/// </summary>
public sealed class TrackEditViewModel
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

    [Display(Name = "مدت (ثانیه)")]
    public int? DurationSeconds { get; set; }

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

    [Display(Name = "نوع در دسترس بودن متن ترانه")]
    public int? LyricsAvailabilityTypeId { get; set; }

    [Display(Name = "سبک آواز")]
    public int? VocalStyleId { get; set; }

    [Display(Name = "گام موسیقی")]
    public int? MusicalKeyId { get; set; }

    [Display(Name = "BPM")]
    public short? BPM { get; set; }

    [StringLength(12)]
    [Display(Name = "ISRC")]
    public string? ISRC { get; set; }

    [Display(Name = "بی‌کلام است")]
    public bool IsInstrumental { get; set; }

    [Display(Name = "صریح است")]
    public bool IsExplicit { get; set; }

    [StringLength(1000)]
    [Display(Name = "اعلامیه حق نشر")]
    public string? CopyrightNotice { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    // ──────────────────────────────────────────────
    // Concurrency / Audit
    // ──────────────────────────────────────────────

    /// <summary>RowVersion for concurrency checking (Edit only).</summary>
    public byte[]? RowVersion { get; set; }

    /// <summary>Track ID (0 for create, >0 for edit).</summary>
    public int TrackId { get; set; }

    /// <summary>The polymorphic EntityType id for tracks (used by related-editor tabs).</summary>
    public int EntityTypeId => 2;

    // ──────────────────────────────────────────────
    // Dropdown Data
    // ──────────────────────────────────────────────

    /// <summary>Available lyrics availability types.</summary>
    public IReadOnlyList<LyricsAvailabilityType> LyricsAvailabilityTypes { get; set; } = [];

    /// <summary>Available vocal styles.</summary>
    public IReadOnlyList<VocalStyle> VocalStyles { get; set; } = [];

    /// <summary>Available musical keys.</summary>
    public IReadOnlyList<MusicalKey> MusicalKeys { get; set; } = [];
}
