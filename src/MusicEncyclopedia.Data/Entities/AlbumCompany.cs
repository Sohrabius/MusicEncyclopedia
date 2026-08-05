using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumCompany")]
public class AlbumCompany
{
    [Key]
    public int AlbumCompanyId { get; set; }

    public int AlbumId { get; set; }

    public int CompanyId { get; set; }

    public int? CompanyRoleTypeId { get; set; }

    [StringLength(100)]
    public string? CatalogNumber { get; set; }

    [StringLength(50)]
    public string? Barcode { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(CompanyId))]
    public virtual Company Company { get; set; } = null!;

    [ForeignKey(nameof(CompanyRoleTypeId))]
    public virtual CompanyRoleType? CompanyRoleType { get; set; }
}
