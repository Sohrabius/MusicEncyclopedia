using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumCountry")]
public class AlbumCountry
{
    [Key]
    public int AlbumCountryId { get; set; }

    public int AlbumId { get; set; }

    public int? CountryId { get; set; }

    public int? CountryRoleTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(CountryId))]
    public virtual Country? Country { get; set; }

    [ForeignKey(nameof(CountryRoleTypeId))]
    public virtual CountryRoleType? CountryRoleType { get; set; }
}
