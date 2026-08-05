using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Credit")]
public class Credit
{
    [Key]
    public int CreditId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int CreditRoleId { get; set; }

    public int RoleScopeTypeId { get; set; }

    public int? PersonId { get; set; }

    public int? CompanyId { get; set; }

    public int? InstrumentId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public string? Notes { get; set; }

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(CreditRoleId))]
    public virtual CreditRole CreditRole { get; set; } = null!;

    [ForeignKey(nameof(RoleScopeTypeId))]
    public virtual RoleScopeType RoleScopeType { get; set; } = null!;

    [ForeignKey(nameof(PersonId))]
    public virtual Person? Person { get; set; }

    [ForeignKey(nameof(CompanyId))]
    public virtual Company? Company { get; set; }

    [ForeignKey(nameof(InstrumentId))]
    public virtual Instrument? Instrument { get; set; }
}
