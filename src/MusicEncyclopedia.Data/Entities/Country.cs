using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Country")]
public class Country
{
    [Key]
    public int CountryId { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<Person> PeopleAsNationality { get; set; } = new List<Person>();
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    public virtual ICollection<Company> Companies { get; set; } = new List<Company>();
    public virtual ICollection<AlbumCountry> AlbumCountries { get; set; } = new List<AlbumCountry>();
    public virtual ICollection<Instrument> Instruments { get; set; } = new List<Instrument>();
    public virtual ICollection<Award> Awards { get; set; } = new List<Award>();
    public virtual ICollection<Certification> Certifications { get; set; } = new List<Certification>();
    public virtual ICollection<Chart> Charts { get; set; } = new List<Chart>();
}
