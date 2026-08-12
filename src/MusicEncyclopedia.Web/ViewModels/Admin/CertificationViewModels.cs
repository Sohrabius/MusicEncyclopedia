using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin certification list page.
/// </summary>
public sealed class CertificationListViewModel
{
    public PagedResult<CertificationListItem> Items { get; init; } = PagedResult<CertificationListItem>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>A row in the admin certification list.</summary>
public sealed class CertificationListItem
{
    public int CertificationId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Organization { get; init; }
    public string? CountryName { get; init; }
}

/// <summary>
/// View model for the certification create/edit form.
/// </summary>
public sealed class CertificationEditViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "سازمان")]
    public string? Organization { get; set; }

    [Display(Name = "کشور")]
    public int? CountryId { get; set; }

    [Display(Name = "توضیحات")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255)]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int CertificationId { get; set; }

    public IReadOnlyList<Country> Countries { get; set; } = [];
}
