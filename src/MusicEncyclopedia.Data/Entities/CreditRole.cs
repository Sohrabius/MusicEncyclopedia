using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("CreditRole")]
public class CreditRole
{
    [Key]
    public int CreditRoleId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int DisplayOrder { get; set; }

    public int RoleScopeTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(RoleScopeTypeId))]
    public virtual RoleScopeType RoleScopeType { get; set; } = null!;

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
    public virtual ICollection<CreditRoleEntityType> CreditRoleEntityTypes { get; set; } = new List<CreditRoleEntityType>();
}
