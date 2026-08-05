using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the sung version create/edit form (spec 10.10).
/// Maps to/from the <see cref="SungVersion"/> entity.
/// </summary>
public sealed class SungVersionEditViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Poem is required.")]
    [Display(Name = "Poem")]
    public int PoemId { get; set; }

    [Display(Name = "Vocal Style")]
    public int? VocalStyleId { get; set; }

    [Display(Name = "Text")]
    public string? Text { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Display(Name = "Is Canonical")]
    public bool IsCanonical { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int SungVersionId { get; set; }

    public IReadOnlyList<Poem> Poems { get; set; } = [];
    public IReadOnlyList<VocalStyle> VocalStyles { get; set; } = [];
}
