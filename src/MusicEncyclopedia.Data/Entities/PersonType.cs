using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("PersonType")]
public class PersonType
{
    [Key]
    public int PersonTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<PersonTypeAssignment> PersonTypeAssignments { get; set; } = new List<PersonTypeAssignment>();
}
