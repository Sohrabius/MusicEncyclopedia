using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Citation")]
public class Citation
{
    [Key]
    public int CitationId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int? SourceId { get; set; }

    [StringLength(255)]
    public string? FieldName { get; set; }

    public string? Quote { get; set; }

    [StringLength(50)]
    public string? PageNumber { get; set; }

    [StringLength(2000)]
    public string? Url { get; set; }

    public DateOnly? AccessedDate { get; set; }

    public string? Notes { get; set; }

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(SourceId))]
    public virtual Source? Source { get; set; }
}
