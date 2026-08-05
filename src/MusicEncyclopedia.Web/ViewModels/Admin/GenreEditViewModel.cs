using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the genre create/edit form.
/// Maps to/from the <see cref="Genre"/> entity.
/// </summary>
public sealed class GenreEditViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    [StringLength(255)]
    [Display(Name = "Sort Name")]
    public string? NameSort { get; set; }

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Parent Genre")]
    public int? ParentGenreId { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int GenreId { get; set; }

    public IReadOnlyList<Genre> ParentGenres { get; set; } = [];
}
