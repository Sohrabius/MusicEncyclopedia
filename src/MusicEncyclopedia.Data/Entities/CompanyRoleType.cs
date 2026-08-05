using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("CompanyRoleType")]
public class CompanyRoleType
{
    [Key]
    public int CompanyRoleTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<AlbumCompany> AlbumCompanies { get; set; } = new List<AlbumCompany>();
}
