using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("PerformanceEvent")]
public class PerformanceEvent
{
    [Key]
    public int PerformanceEventId { get; set; }

    public int EntityId { get; set; }

    public int? EventTypeId { get; set; }

    public int? LocationId { get; set; }

    public DateTime? Date { get; set; }

    public string? AudienceInfo { get; set; }

    public string? PerformanceNotes { get; set; }

    public string? ImprovisationNotes { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;

    [ForeignKey(nameof(EventTypeId))]
    public virtual EventType? EventType { get; set; }

    [ForeignKey(nameof(LocationId))]
    public virtual Location? Location { get; set; }

    public virtual ICollection<PerformanceEventAlbum> PerformanceEventAlbums { get; set; } = new List<PerformanceEventAlbum>();
    public virtual ICollection<PerformanceEventTrack> PerformanceEventTracks { get; set; } = new List<PerformanceEventTrack>();
}
