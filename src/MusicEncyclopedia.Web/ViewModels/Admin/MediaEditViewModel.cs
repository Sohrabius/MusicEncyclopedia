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
    [Display(Name = "File Name")]
    public string FileName { get; set; } = "";

    [Display(Name = "Media Type")]
    public int? MediaTypeId { get; set; }

    [StringLength(2000, ErrorMessage = "URL must not exceed 2000 characters.")]
    [Display(Name = "URL")]
    public string? Url { get; set; }

    [Display(Name = "Alt Text")]
    public string? AltText { get; set; }

    [Display(Name = "Width (px)")]
    public int? Width { get; set; }

    [Display(Name = "Height (px)")]
    public int? Height { get; set; }

    [Display(Name = "File Size (bytes)")]
    public long FileSize { get; set; }

    [Required(ErrorMessage = "MIME type is required.")]
    [StringLength(50, ErrorMessage = "MIME type must not exceed 50 characters.")]
    [Display(Name = "MIME Type")]
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
    [Display(Name = "File")]
    public IFormFile? File { get; set; }

    [Display(Name = "Media Type")]
    public int? MediaTypeId { get; set; }

    [StringLength(500, ErrorMessage = "File name must not exceed 500 characters.")]
    [Display(Name = "File Name (optional, defaults to uploaded file name)")]
    public string? FileName { get; set; }

    [Display(Name = "Alt Text")]
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
    [Display(Name = "Entity Type")]
    public int EntityTypeId { get; set; }

    [Required(ErrorMessage = "Entity ID is required.")]
    [Display(Name = "Entity ID")]
    public int EntityId { get; set; }

    [Display(Name = "Media Role")]
    public int? MediaRoleTypeId { get; set; }

    [Display(Name = "Primary")]
    public bool IsPrimary { get; set; }

    [Display(Name = "Display Order")]
    public int DisplayOrder { get; set; }

    public IReadOnlyList<MediaRoleType> MediaRoleTypes { get; set; } = [];
    public IReadOnlyList<EntityType> EntityTypes { get; set; } = [];
}
