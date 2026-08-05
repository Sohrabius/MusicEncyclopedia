using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AwardResultType")]
public class AwardResultType
{
    [Key]
    public int AwardResultTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<AwardAssignment> AwardAssignments { get; set; } = new List<AwardAssignment>();
}
