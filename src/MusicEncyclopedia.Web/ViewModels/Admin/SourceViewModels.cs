using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Core.DTOs;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin source list page.
/// </summary>
public sealed class SourceListViewModel
{
    public PagedResult<SourceListItem> Items { get; init; } = PagedResult<SourceListItem>.Create([], 1, 20, 0);
    public string? SearchQuery { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>A row in the admin source list.</summary>
public sealed class SourceListItem
{
    public int SourceId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? TypeName { get; init; }
    public string? Author { get; init; }
}

/// <summary>
/// View model for the source create/edit form.
/// </summary>
public sealed class SourceEditViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = "";

    [Display(Name = "Source Type")]
    public int? SourceTypeId { get; set; }

    [StringLength(500)]
    [Display(Name = "Author")]
    public string? Author { get; set; }

    [Display(Name = "Publisher")]
    public int? PublisherId { get; set; }

    [Display(Name = "Publication Date")]
    public DateOnly? PublicationDate { get; set; }

    [StringLength(2000)]
    [Display(Name = "URL")]
    public string? Url { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255)]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int SourceId { get; set; }

    public IReadOnlyList<SourceType> SourceTypes { get; set; } = [];
    public IReadOnlyList<Company> Companies { get; set; } = [];
}
