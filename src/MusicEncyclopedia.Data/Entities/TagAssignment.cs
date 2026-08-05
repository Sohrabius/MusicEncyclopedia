using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TagAssignment")]
public class TagAssignment
{
    [Key]
    public int TagAssignmentId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int TagId { get; set; }

    // Navigation
    [ForeignKey(nameof(TagId))]
    public virtual Tag Tag { get; set; } = null!;
}
