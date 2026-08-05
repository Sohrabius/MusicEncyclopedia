using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("MediaRoleType")]
public class MediaRoleType
{
    [Key]
    public int MediaRoleTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<MediaAssignment> MediaAssignments { get; set; } = new List<MediaAssignment>();
}
