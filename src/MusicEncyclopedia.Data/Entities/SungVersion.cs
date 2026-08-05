using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("SungVersion")]
public class SungVersion
{
    [Key]
    public int SungVersionId { get; set; }

    public int EntityId { get; set; }

    public int PoemId { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    public int? VocalStyleId { get; set; }

    public string? Text { get; set; }

    public string? Notes { get; set; }

    public bool IsCanonical { get; set; }

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

    [ForeignKey(nameof(PoemId))]
    public virtual Poem Poem { get; set; } = null!;

    [ForeignKey(nameof(VocalStyleId))]
    public virtual VocalStyle? VocalStyle { get; set; }

    public virtual ICollection<TrackSungVersion> TrackSungVersions { get; set; } = new List<TrackSungVersion>();
}
