using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("CreditRoleEntityType")]
public class CreditRoleEntityType
{
    [Key]
    public int CreditRoleEntityTypeId { get; set; }

    public int CreditRoleId { get; set; }

    public int EntityTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(CreditRoleId))]
    public virtual CreditRole CreditRole { get; set; } = null!;

    [ForeignKey(nameof(EntityTypeId))]
    public virtual EntityType EntityType { get; set; } = null!;
}
