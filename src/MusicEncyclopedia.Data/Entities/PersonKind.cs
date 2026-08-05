using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("PersonKind")]
public class PersonKind
{
    [Key]
    public int PersonKindId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<Person> People { get; set; } = new List<Person>();
}
