using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the poem create/edit form (spec 10.9).
/// Maps to/from the <see cref="Poem"/> entity.
/// </summary>
public sealed class PoemEditViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "Original Title")]
    public string? OriginalTitle { get; set; }

    [StringLength(500)]
    [Display(Name = "English Title")]
    public string? EnglishTitle { get; set; }

    [Display(Name = "Poet")]
    public int? PoetId { get; set; }

    [Display(Name = "Publication")]
    public int? PublicationId { get; set; }

    [Display(Name = "Source")]
    public string? Source { get; set; }

    [StringLength(500)]
    [Display(Name = "Book")]
    public string? Book { get; set; }

    [Display(Name = "Original Publication Date")]
    public DateOnly? OriginalPublicationDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Original Publication Date Precision")]
    public string? OriginalPublicationDatePrecision { get; set; }

    [StringLength(500)]
    [Display(Name = "External Reference URL")]
    public string? ExternalReferenceUrl { get; set; }

    [Display(Name = "Copyright")]
    public string? Copyright { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Canonical Text")]
    public string? CanonicalText { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int PoemId { get; set; }

    public IReadOnlyList<Person> Poets { get; set; } = [];
    public IReadOnlyList<Publication> Publications { get; set; } = [];
}
