using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the media upload/edit form.
/// Maps to/from the <see cref="Media"/> entity.
/// </summary>
public sealed class MediaEditViewModel
{
    public int MediaId { get; set; }

    [Required(ErrorMessage = "File name is required.")]
    [StringLength(500, ErrorMessage = "File name must not exceed 500 characters.")]
    [Display(Name = "نام فایل")]
    public string FileName { get; set; } = "";

    [Display(Name = "نوع رسانه")]
    public int? MediaTypeId { get; set; }

    [StringLength(2000, ErrorMessage = "URL must not exceed 2000 characters.")]
    [Display(Name = "URL")]
    public string? Url { get; set; }

    [Display(Name = "متن جایگزین")]
    public string? AltText { get; set; }

    [Display(Name = "عرض (پیکسل)")]
    public int? Width { get; set; }

    [Display(Name = "ارتفاع (پیکسل)")]
    public int? Height { get; set; }

    [Display(Name = "اندازه فایل (بایت)")]
    public long FileSize { get; set; }

    [Required(ErrorMessage = "MIME type is required.")]
    [StringLength(50, ErrorMessage = "MIME type must not exceed 50 characters.")]
    [Display(Name = "نوع MIME")]
    public string MimeType { get; set; } = "";

    public string? FilePath { get; set; }
    public string? ThumbnailUrl150 { get; set; }
    public string? ThumbnailUrl300 { get; set; }
    public string? ThumbnailUrl600 { get; set; }
    public string? ThumbnailUrl1200 { get; set; }

    // Dropdown data
    public IReadOnlyList<MediaType> MediaTypes { get; set; } = [];
}

/// <summary>
/// View model for the media upload form.
/// </summary>
public sealed class MediaUploadViewModel
{
    [Required(ErrorMessage = "Please select a file to upload.")]
    [Display(Name = "فایل")]
    public IFormFile? File { get; set; }

    [Display(Name = "نوع رسانه")]
    public int? MediaTypeId { get; set; }

    [StringLength(500, ErrorMessage = "File name must not exceed 500 characters.")]
    [Display(Name = "نام فایل (اختیاری؛ در صورت خالی بودن، نام فایل بارگذاری‌شده استفاده می‌شود)")]
    public string? FileName { get; set; }

    [Display(Name = "متن جایگزین")]
    public string? AltText { get; set; }

    // Dropdown data
    public IReadOnlyList<MediaType> MediaTypes { get; set; } = [];
}

/// <summary>
/// View model for assigning media to an entity.
/// </summary>
public sealed class MediaAssignViewModel
{
    [Required]
    public int MediaId { get; set; }

    [Required(ErrorMessage = "Entity type is required.")]
    [Display(Name = "نوع موجودیت")]
    public int EntityTypeId { get; set; }

    [Required(ErrorMessage = "Entity ID is required.")]
    [Display(Name = "شناسه موجودیت")]
    public int EntityId { get; set; }

    [Display(Name = "نقش رسانه")]
    public int? MediaRoleTypeId { get; set; }

    [Display(Name = "اصلی")]
    public bool IsPrimary { get; set; }

    [Display(Name = "ترتیب نمایش")]
    public int DisplayOrder { get; set; }

    public IReadOnlyList<MediaRoleType> MediaRoleTypes { get; set; } = [];
    public IReadOnlyList<EntityType> EntityTypes { get; set; } = [];
}
