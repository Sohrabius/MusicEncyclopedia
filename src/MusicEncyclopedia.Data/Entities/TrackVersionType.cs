using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackVersionType")]
public class TrackVersionType
{
    [Key]
    public int TrackVersionTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<TrackVersionTypeAssignment> TrackVersionTypeAssignments { get; set; } = new List<TrackVersionTypeAssignment>();
}
