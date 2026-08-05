using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("MusicianInstrument")]
public class MusicianInstrument
{
    [Key]
    public int MusicianInstrumentId { get; set; }

    public int PersonId { get; set; }

    public int InstrumentId { get; set; }

    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(PersonId))]
    public virtual Person Person { get; set; } = null!;

    [ForeignKey(nameof(InstrumentId))]
    public virtual Instrument Instrument { get; set; } = null!;
}
