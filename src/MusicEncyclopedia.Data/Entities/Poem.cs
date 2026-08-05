using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Poem")]
public class Poem
{
    [Key]
    public int PoemId { get; set; }

    public int EntityId { get; set; }

    public int? PersonId { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? OriginalTitle { get; set; }

    [StringLength(500)]
    public string? EnglishTitle { get; set; }

    public int? PublicationId { get; set; }

    public string? Source { get; set; }

    [StringLength(500)]
    public string? Book { get; set; }

    public DateOnly? OriginalPublicationDate { get; set; }

    [StringLength(50)]
    public string? OriginalPublicationDatePrecision { get; set; }

    [StringLength(500)]
    public string? ExternalReferenceUrl { get; set; }

    public string? Copyright { get; set; }

    public string? Notes { get; set; }

    public string? CanonicalText { get; set; }

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
    public virtual Person? Person { get; set; }

    [ForeignKey(nameof(PublicationId))]
    public virtual Publication? Publication { get; set; }

    public virtual ICollection<SungVersion> SungVersions { get; set; } = new List<SungVersion>();
}
