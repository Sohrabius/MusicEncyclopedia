using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Company")]
public class Company
{
    [Key]
    public int CompanyId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(500)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? NameSort { get; set; }

    [StringLength(500)]
    public string? OriginalName { get; set; }

    [StringLength(500)]
    public string? EnglishName { get; set; }

    public int? CompanyTypeId { get; set; }

    public int? CountryId { get; set; }

    [StringLength(500)]
    public string? Website { get; set; }

    public string? History { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;

    [ForeignKey(nameof(CompanyTypeId))]
    public virtual CompanyType? CompanyType { get; set; }

    [ForeignKey(nameof(CountryId))]
    public virtual Country? Country { get; set; }

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
    public virtual ICollection<AlbumCompany> AlbumCompanies { get; set; } = new List<AlbumCompany>();
    public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();
    public virtual ICollection<Source> Sources { get; set; } = new List<Source>();
}
