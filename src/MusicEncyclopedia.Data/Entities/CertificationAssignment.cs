using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("CertificationAssignment")]
public class CertificationAssignment
{
    [Key]
    public int CertificationAssignmentId { get; set; }

    public int CertificationId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    [StringLength(100)]
    public string? CertificationLevel { get; set; }

    public DateOnly? Date { get; set; }

    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(CertificationId))]
    public virtual Certification Certification { get; set; } = null!;
}
