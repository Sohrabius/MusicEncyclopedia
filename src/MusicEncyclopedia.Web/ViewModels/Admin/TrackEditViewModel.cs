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

    [Display(Name = "Duration (seconds)")]
    public int? DurationSeconds { get; set; }

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

    [Display(Name = "Lyrics Availability Type")]
    public int? LyricsAvailabilityTypeId { get; set; }

    [Display(Name = "Vocal Style")]
    public int? VocalStyleId { get; set; }

    [Display(Name = "Musical Key")]
    public int? MusicalKeyId { get; set; }

    [Display(Name = "BPM")]
    public short? BPM { get; set; }

    [StringLength(12)]
    [Display(Name = "ISRC")]
    public string? ISRC { get; set; }

    [Display(Name = "Is Instrumental")]
    public bool IsInstrumental { get; set; }

    [Display(Name = "Is Explicit")]
    public bool IsExplicit { get; set; }

    [StringLength(1000)]
    [Display(Name = "Copyright Notice")]
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
