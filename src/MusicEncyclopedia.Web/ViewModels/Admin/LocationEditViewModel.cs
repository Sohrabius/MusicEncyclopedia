using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class LocationEditViewModel
{
    public int LocationId { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "نام")]
    public string Name { get; set; } = "";

    [Display(Name = "نوع مکان")]
    public int? LocationTypeId { get; set; }

    [Display(Name = "مکان والد")]
    public int? ParentLocationId { get; set; }

    [Display(Name = "کشور")]
    public int? CountryId { get; set; }

    [Display(Name = "عرض جغرافیایی")]
    public decimal? Latitude { get; set; }

    [Display(Name = "طول جغرافیایی")]
    public decimal? Longitude { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }

    // Dropdown data
    public IReadOnlyList<LocationType> LocationTypes { get; set; } = [];
    public IReadOnlyList<Location> ParentLocations { get; set; } = [];
    public IReadOnlyList<Country> Countries { get; set; } = [];
}
