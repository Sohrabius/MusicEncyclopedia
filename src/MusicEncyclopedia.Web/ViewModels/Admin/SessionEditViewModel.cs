using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class SessionEditViewModel
{
    public int RecordingSessionId { get; set; }

    [Display(Name = "Session Type")]
    public int? SessionTypeId { get; set; }

    [Display(Name = "Location")]
    public int? LocationId { get; set; }

    [Display(Name = "Start Date")]
    public DateOnly? StartDate { get; set; }

    [Display(Name = "End Date")]
    public DateOnly? EndDate { get; set; }

    [StringLength(50)]
    [Display(Name = "Date Precision")]
    public string? DatePrecision { get; set; }

    [Display(Name = "Notes")]
    public string? Notes { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }

    // Dropdown data
    public IReadOnlyList<SessionType> SessionTypes { get; set; } = [];
    public IReadOnlyList<Location> Locations { get; set; } = [];
}
