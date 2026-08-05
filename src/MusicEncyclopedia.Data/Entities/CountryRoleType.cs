using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("CountryRoleType")]
public class CountryRoleType
{
    [Key]
    public int CountryRoleTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<AlbumCountry> AlbumCountries { get; set; } = new List<AlbumCountry>();
}
