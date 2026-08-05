using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("MediaAssignment")]
public class MediaAssignment
{
    [Key]
    public int MediaAssignmentId { get; set; }

    public int MediaId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int? MediaRoleTypeId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    // Navigation
    [ForeignKey(nameof(MediaId))]
    public virtual Media Media { get; set; } = null!;

    [ForeignKey(nameof(MediaRoleTypeId))]
    public virtual MediaRoleType? MediaRoleType { get; set; }
}
