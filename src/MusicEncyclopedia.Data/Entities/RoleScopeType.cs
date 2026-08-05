using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("RoleScopeType")]
public class RoleScopeType
{
    [Key]
    public int RoleScopeTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<CreditRole> CreditRoles { get; set; } = new List<CreditRole>();
    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
}
