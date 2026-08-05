using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AwardAssignment")]
public class AwardAssignment
{
    [Key]
    public int AwardAssignmentId { get; set; }

    public int AwardId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int? AwardResultTypeId { get; set; }

    public int? Year { get; set; }

    [StringLength(500)]
    public string? Category { get; set; }

    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(AwardId))]
    public virtual Award Award { get; set; } = null!;

    [ForeignKey(nameof(AwardResultTypeId))]
    public virtual AwardResultType? AwardResultType { get; set; }
}
