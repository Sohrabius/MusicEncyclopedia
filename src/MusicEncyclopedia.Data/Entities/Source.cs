using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Source")]
public class Source
{
    [Key]
    public int SourceId { get; set; }

    public int EntityId { get; set; }

    public int? SourceTypeId { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? Author { get; set; }

    public int? PublisherId { get; set; }

    public DateOnly? PublicationDate { get; set; }

    [StringLength(2000)]
    public string? Url { get; set; }

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

    [ForeignKey(nameof(SourceTypeId))]
    public virtual SourceType? SourceType { get; set; }

    [ForeignKey(nameof(PublisherId))]
    public virtual Company? Publisher { get; set; }

    public virtual ICollection<Citation> Citations { get; set; } = new List<Citation>();
}
