using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Person")]
public class Person
{
    [Key]
    public int PersonId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(500)]
    public string FullName { get; set; } = null!;

    [StringLength(500)]
    public string? FullNameSort { get; set; }

    [StringLength(500)]
    public string? OriginalName { get; set; }

    [StringLength(500)]
    public string? EnglishName { get; set; }

    public int? PersonKindId { get; set; }

    public string? Biography { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(50)]
    public string? BirthDatePrecision { get; set; }

    public int? BirthLocationId { get; set; }

    public DateOnly? DeathDate { get; set; }

    [StringLength(50)]
    public string? DeathDatePrecision { get; set; }

    public int? DeathLocationId { get; set; }

    public int? NationalityCountryId { get; set; }

    public int? ImageMediaId { get; set; }

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

    [ForeignKey(nameof(PersonKindId))]
    public virtual PersonKind? PersonKind { get; set; }

    [ForeignKey(nameof(BirthLocationId))]
    public virtual Location? BirthLocation { get; set; }

    [ForeignKey(nameof(DeathLocationId))]
    public virtual Location? DeathLocation { get; set; }

    [ForeignKey(nameof(NationalityCountryId))]
    public virtual Country? NationalityCountry { get; set; }

    [ForeignKey(nameof(ImageMediaId))]
    public virtual Media? ImageMedia { get; set; }

    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
    public virtual ICollection<MusicianInstrument> MusicianInstruments { get; set; } = new List<MusicianInstrument>();
    public virtual ICollection<PersonTypeAssignment> PersonTypeAssignments { get; set; } = new List<PersonTypeAssignment>();
    public virtual ICollection<Poem> Poems { get; set; } = new List<Poem>();
    public virtual ICollection<Publication> Publications { get; set; } = new List<Publication>();
}
