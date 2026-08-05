using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("EntityType")]
public class EntityType
{
    [Key]
    public int EntityTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<Entity> Entities { get; set; } = new List<Entity>();
    public virtual ICollection<CreditRoleEntityType> CreditRoleEntityTypes { get; set; } = new List<CreditRoleEntityType>();
}
