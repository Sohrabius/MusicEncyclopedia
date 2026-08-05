using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the instrument create/edit form.
/// Maps to/from the <see cref="Instrument"/> entity.
/// </summary>
public sealed class InstrumentEditViewModel
{
    [Required(ErrorMessage = "Name is required.")]
    [StringLength(255, ErrorMessage = "Name must not exceed 255 characters.")]
    [Display(Name = "Name")]
    public string Name { get; set; } = "";

    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Instrument Family")]
    public int? InstrumentFamilyId { get; set; }

    [Display(Name = "Country")]
    public int? CountryId { get; set; }

    [Display(Name = "Historical Notes")]
    public string? HistoricalNotes { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }
    public int InstrumentId { get; set; }

    public IReadOnlyList<InstrumentFamily> InstrumentFamilies { get; set; } = [];
    public IReadOnlyList<Country> Countries { get; set; } = [];
}
