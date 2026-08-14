using System.ComponentModel.DataAnnotations;
using MediaEntity = MusicEncyclopedia.Data.Entities.Media;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the album creation wizard (single atomic form, client stepper).
/// Everything the wizard collects — album fields, classification joins, tracks with
/// their full nested dependencies, credits, tags, links and related albums — is
/// bound here and persisted in one POST.
/// </summary>
public sealed class AlbumWizardViewModel
{
    // ──────────────────────────────────────────────
    // Identity — 0 means the wizard is in create mode;
    // a positive value loads that album for step-by-step editing.
    // ──────────────────────────────────────────────

    [Display(Name = "شناسه آلبوم")]
    public int AlbumId { get; set; }

    // ──────────────────────────────────────────────
    // Step 1 — Album basics
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

    [StringLength(255)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    [Display(Name = "رسمی است")]
    public bool IsOfficial { get; set; }

    // ──────────────────────────────────────────────
    // Step 2 — Album classification & distribution
    // ──────────────────────────────────────────────

    public List<int> GenreIds { get; set; } = [];
    public List<int> MoodIds { get; set; } = [];
    public List<int> LanguageIds { get; set; } = [];
    public List<int> CountryIds { get; set; } = [];
    public List<CompanyWizardItem> Companies { get; set; } = [];
    public List<IdentifierWizardItem> Identifiers { get; set; } = [];

    // ──────────────────────────────────────────────
    // Step 3 — Tracks (full per-track editor)
    // ──────────────────────────────────────────────

    public List<TrackWizardItem> Tracks { get; set; } = [];

    // ──────────────────────────────────────────────
    // Step 4 — Album credits
    // ──────────────────────────────────────────────

    public List<CreditWizardItem> AlbumCredits { get; set; } = [];

    // ──────────────────────────────────────────────
    // Step 5 — Tags, links & related albums
    // ──────────────────────────────────────────────

    public List<int> TagIds { get; set; } = [];
    public List<LinkWizardItem> Links { get; set; } = [];
    public List<RelatedAlbumWizardItem> RelatedAlbums { get; set; } = [];

    // ──────────────────────────────────────────────
    // UI state
    // ──────────────────────────────────────────────

    /// <summary>Step to open after a server round-trip (1-6).</summary>
    public int ActiveStep { get; set; } = 1;

    // ──────────────────────────────────────────────
    // Dropdown data (populated on GET; re-populated on validation failure)
    // ──────────────────────────────────────────────

    public IReadOnlyList<AlbumCategory> Categories { get; set; } = [];
    public IReadOnlyList<Genre> GenreOptions { get; set; } = [];
    public IReadOnlyList<Mood> MoodOptions { get; set; } = [];
    public IReadOnlyList<Language> LanguageOptions { get; set; } = [];
    public IReadOnlyList<Country> CountryOptions { get; set; } = [];
    public IReadOnlyList<Company> CompanyOptions { get; set; } = [];
    public IReadOnlyList<CompanyRoleType> CompanyRoleTypeOptions { get; set; } = [];
    public IReadOnlyList<IdentifierType> IdentifierTypeOptions { get; set; } = [];
    public IReadOnlyList<Instrument> InstrumentOptions { get; set; } = [];
    public IReadOnlyList<VocalStyle> VocalStyleOptions { get; set; } = [];
    public IReadOnlyList<MusicalKey> MusicalKeyOptions { get; set; } = [];
    public IReadOnlyList<LyricsAvailabilityType> LyricsAvailabilityTypeOptions { get; set; } = [];
    public IReadOnlyList<TrackVersionType> TrackVersionTypeOptions { get; set; } = [];
    public IReadOnlyList<CreditRole> CreditRoleOptions { get; set; } = [];
    public IReadOnlyList<RoleScopeType> RoleScopeTypeOptions { get; set; } = [];
    public IReadOnlyList<Person> PersonOptions { get; set; } = [];
    public IReadOnlyList<Poem> PoemOptions { get; set; } = [];
    public IReadOnlyList<SungVersion> SungVersionOptions { get; set; } = [];
    public IReadOnlyList<Tag> TagOptions { get; set; } = [];
    public IReadOnlyList<LinkType> LinkTypeOptions { get; set; } = [];
    public IReadOnlyList<Album> AlbumOptions { get; set; } = [];
    public IReadOnlyList<AlbumRelationType> AlbumRelationTypeOptions { get; set; } = [];
    public IReadOnlyList<MediaEntity> MediaOptions { get; set; } = [];
}

/// <summary>
/// One track card in the wizard: track fields + album-track row flags + the
/// full set of nested dependencies (assigns, sung versions, credits).
/// </summary>
public sealed class TrackWizardItem
{
    /// <summary>Existing track id when editing an album; 0 for a brand-new track.</summary>
    [Display(Name = "شناسه ترک")]
    public int TrackId { get; set; }

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

    [StringLength(255)]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    // ── Album-track row flags ────────────────────

    [Display(Name = "دیسک")]
    public int DiscNumber { get; set; } = 1;

    [Display(Name = "شماره")]
    public int TrackNumber { get; set; }

    [Display(Name = "آهنگ جایزه")]
    public bool IsBonus { get; set; }

    [Display(Name = "مخفی")]
    public bool IsHidden { get; set; }

    // ── Assigns (multi-select) ───────────────────

    public List<int> GenreIds { get; set; } = [];
    public List<int> MoodIds { get; set; } = [];
    public List<int> InstrumentIds { get; set; } = [];
    public List<int> VersionTypeIds { get; set; } = [];

    // ── Nested dependencies ──────────────────────

    public List<SungVersionWizardItem> SungVersions { get; set; } = [];
    public List<CreditWizardItem> Credits { get; set; } = [];
}

/// <summary>
/// A sung-version row inside a track card: either pick an existing sung version,
/// or create a new one (Title + Poem are required in that branch).
/// </summary>
public sealed class SungVersionWizardItem
{
    [Display(Name = "نسخه خوانده‌شده موجود")]
    public int? SungVersionId { get; set; }

    [StringLength(500)]
    [Display(Name = "عنوان")]
    public string? Title { get; set; }

    [Display(Name = "شعر")]
    public int? PoemId { get; set; }

    [Display(Name = "سبک آواز")]
    public int? VocalStyleId { get; set; }

    [Display(Name = "نسخه اصلی است")]
    public bool IsCanonical { get; set; }

    [Display(Name = "متن")]
    public string? Text { get; set; }

    [Display(Name = "یادداشت‌ها")]
    public string? Notes { get; set; }

    /// <summary>True when a new sung version should be created rather than reused.</summary>
    public bool IsNew => SungVersionId is null;
}

/// <summary>
/// A credit row (album-level or per-track). Polymorphic: one of Person/Company/
/// Instrument must be chosen.
/// </summary>
public sealed class CreditWizardItem
{
    [Display(Name = "نقش")]
    public int CreditRoleId { get; set; }

    [Display(Name = "حوزه")]
    public int RoleScopeTypeId { get; set; }

    [Display(Name = "شخص")]
    public int? PersonId { get; set; }

    [Display(Name = "شرکت")]
    public int? CompanyId { get; set; }

    [Display(Name = "ساز")]
    public int? InstrumentId { get; set; }

    [Display(Name = "ترتیب نمایش")]
    public int DisplayOrder { get; set; }

    [Display(Name = "اصلی")]
    public bool IsPrimary { get; set; }

    [Display(Name = "یادداشت‌ها")]
    public string? Notes { get; set; }
}

/// <summary>Album distribution company row → AlbumCompany.</summary>
public sealed class CompanyWizardItem
{
    [Display(Name = "شرکت")]
    public int CompanyId { get; set; }

    [Display(Name = "نقش شرکت")]
    public int? CompanyRoleTypeId { get; set; }

    [StringLength(100)]
    [Display(Name = "شماره کاتالوگ")]
    public string? CatalogNumber { get; set; }

    [StringLength(50)]
    [Display(Name = "بارکد")]
    public string? Barcode { get; set; }
}

/// <summary>Album identifier row → AlbumIdentifier.</summary>
public sealed class IdentifierWizardItem
{
    [Display(Name = "نوع شناسه")]
    public int? IdentifierTypeId { get; set; }

    [StringLength(255)]
    [Display(Name = "مقدار")]
    public string Value { get; set; } = "";
}

/// <summary>External link row → EntityLink.</summary>
public sealed class LinkWizardItem
{
    [StringLength(2000)]
    [Display(Name = "URL")]
    public string Url { get; set; } = "";

    [Display(Name = "نوع پیوند")]
    public int? LinkTypeId { get; set; }

    [StringLength(500)]
    [Display(Name = "عنوان")]
    public string? Label { get; set; }
}

/// <summary>Related album row → AlbumRelation (inserted in both directions).</summary>
public sealed class RelatedAlbumWizardItem
{
    [Display(Name = "آلبوم")]
    public int AlbumId { get; set; }

    [Display(Name = "نوع رابطه")]
    public int AlbumRelationTypeId { get; set; }
}

/// <summary>
/// Payload for the wizard's "quick add" endpoint — creates a small entity (person,
/// company, poem, genre, …) or Code+Name lookup row on the fly so it can be selected
/// immediately in the current wizard form.
/// </summary>
public sealed class QuickAddFormModel
{
    /// <summary>Lower-case target key: person, company, poem, genre, mood, instrument, tag, albumcategory, language, country, trackversiontype.</summary>
    public string Target { get; set; } = "";

    /// <summary>Display name — FullName for person, Title for poem, Name for everything else.</summary>
    public string? Name { get; set; }

    /// <summary>Code for Code+Name lookup tables (albumcategory, language, country, trackversiontype).</summary>
    public string? Code { get; set; }

    /// <summary>Optional English name (person, company).</summary>
    public string? EnglishName { get; set; }

    /// <summary>Optional poet (person) for a quick-added poem.</summary>
    public int? PoetId { get; set; }
}
