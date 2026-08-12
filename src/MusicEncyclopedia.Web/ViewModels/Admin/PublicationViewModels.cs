using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin publication list page.
/// </summary>
public sealed class PublicationListViewModel
{
    public PagedResult<PublicationListItem> Items { get; init; } = PagedResult<PublicationListItem>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>A row in the admin publication list.</summary>
public sealed class PublicationListItem
{
    public int PublicationId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? TypeName { get; init; }
    public string? PersonName { get; init; }
    public DateOnly? PublicationDate { get; init; }
}

/// <summary>
/// View model for the publication create/edit form.
/// </summary>
public sealed class PublicationEditViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    [Display(Name = "عنوان")]
    public string Title { get; set; } = "";

    [Display(Name = "نوع انتشارات")]
    public int? PublicationTypeId { get; set; }

    [Required(ErrorMessage = "Person is required.")]
    [Display(Name = "شخص")]
    public int PersonId { get; set; }

    [Display(Name = "ناشر")]
    public int? PublisherId { get; set; }

    [Display(Name = "تاریخ انتشار")]
    public DateOnly? PublicationDate { get; set; }

    [StringLength(50)]
    [Display(Name = "دقت تاریخ")]
    public string? PublicationDatePrecision { get; set; }

    [StringLength(50)]
    [Display(Name = "ISBN")]
    public string? ISBN { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255)]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int PublicationId { get; set; }

    public IReadOnlyList<PublicationType> PublicationTypes { get; set; } = [];
    public IReadOnlyList<Person> People { get; set; } = [];
    public IReadOnlyList<Company> Companies { get; set; } = [];
}
