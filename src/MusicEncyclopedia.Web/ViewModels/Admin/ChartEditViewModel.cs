using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class ChartEditViewModel
{
    public int ChartId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    [StringLength(500)]
    [Display(Name = "Publisher")]
    public string? Publisher { get; set; }

    [Display(Name = "Country")]
    public int? CountryId { get; set; }

    [StringLength(50)]
    [Display(Name = "Frequency")]
    public string? Frequency { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }

    // Dropdown data
    public IReadOnlyList<Country> Countries { get; set; } = [];
}
