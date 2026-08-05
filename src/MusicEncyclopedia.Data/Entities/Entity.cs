using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Entity")]
public class Entity
{
    [Key]
    public int EntityId { get; set; }

    public int EntityTypeId { get; set; }

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
    [ForeignKey(nameof(EntityTypeId))]
    public virtual EntityType EntityType { get; set; } = null!;

    public virtual Album? Album { get; set; }
    public virtual Track? Track { get; set; }
    public virtual Person? Person { get; set; }
    public virtual Company? Company { get; set; }
    public virtual Genre? Genre { get; set; }
    public virtual Mood? Mood { get; set; }
    public virtual Instrument? Instrument { get; set; }
    public virtual Poem? Poem { get; set; }
    public virtual SungVersion? SungVersion { get; set; }
    public virtual Publication? Publication { get; set; }
    public virtual RecordingSession? RecordingSession { get; set; }
    public virtual PerformanceEvent? PerformanceEvent { get; set; }
    public virtual Location? Location { get; set; }
    public virtual Tag? Tag { get; set; }
    public virtual Source? Source { get; set; }
    public virtual Award? Award { get; set; }
    public virtual Certification? Certification { get; set; }
    public virtual Chart? Chart { get; set; }
}
