using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class EventEditViewModel
{
    public int PerformanceEventId { get; set; }

    [Display(Name = "نوع رویداد")]
    public int? EventTypeId { get; set; }

    [Display(Name = "مکان / محل برگزاری")]
    public int? LocationId { get; set; }

    [Display(Name = "تاریخ / زمان")]
    public DateTime? Date { get; set; }

    [Display(Name = "اطلاعات مخاطب")]
    public string? AudienceInfo { get; set; }

    [Display(Name = "یادداشت‌های اجرا")]
    public string? PerformanceNotes { get; set; }

    [Display(Name = "یادداشت‌های بداهه‌نوازی")]
    public string? ImprovisationNotes { get; set; }

    [Required(ErrorMessage = "Slug is required.")]
    [StringLength(255, ErrorMessage = "Slug must not exceed 255 characters.")]
    [RegularExpression(@"^[a-z0-9\-]+$", ErrorMessage = "Slug may only contain lowercase letters, digits, and hyphens.")]
    [Display(Name = "Slug")]
    public string Slug { get; set; } = "";

    public byte[]? RowVersion { get; set; }

    // Dropdown data
    public IReadOnlyList<EventType> EventTypes { get; set; } = [];
    public IReadOnlyList<Location> Locations { get; set; } = [];
}
