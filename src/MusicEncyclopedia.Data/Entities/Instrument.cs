using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Instrument")]
public class Instrument
{
    [Key]
    public int InstrumentId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int? InstrumentFamilyId { get; set; }

    public int? CountryId { get; set; }

    public string? HistoricalNotes { get; set; }

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

    [ForeignKey(nameof(InstrumentFamilyId))]
    public virtual InstrumentFamily? InstrumentFamily { get; set; }

    [ForeignKey(nameof(CountryId))]
    public virtual Country? Country { get; set; }

    public virtual ICollection<TrackInstrument> TrackInstruments { get; set; } = new List<TrackInstrument>();
    public virtual ICollection<MusicianInstrument> MusicianInstruments { get; set; } = new List<MusicianInstrument>();
    public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
}
