using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Publication")]
public class Publication
{
    [Key]
    public int PublicationId { get; set; }

    public int EntityId { get; set; }

    public int PersonId { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    public int? PublicationTypeId { get; set; }

    public int? PublisherId { get; set; }

    public DateOnly? PublicationDate { get; set; }

    [StringLength(50)]
    public string? PublicationDatePrecision { get; set; }

    [StringLength(50)]
    public string? ISBN { get; set; }

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

    [ForeignKey(nameof(PersonId))]
    public virtual Person Person { get; set; } = null!;

    [ForeignKey(nameof(PublicationTypeId))]
    public virtual PublicationType? PublicationType { get; set; }

    [ForeignKey(nameof(PublisherId))]
    public virtual Company? Publisher { get; set; }

    public virtual ICollection<Poem> Poems { get; set; } = new List<Poem>();
}
