using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackInstrument")]
public class TrackInstrument
{
    [Key]
    public int TrackInstrumentId { get; set; }

    public int TrackId { get; set; }

    public int InstrumentId { get; set; }

    // Navigation
    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey(nameof(InstrumentId))]
    public virtual Instrument Instrument { get; set; } = null!;
}
