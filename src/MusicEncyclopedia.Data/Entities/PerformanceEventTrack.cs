using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("PerformanceEventTrack")]
public class PerformanceEventTrack
{
    [Key]
    public int PerformanceEventTrackId { get; set; }

    public int PerformanceEventId { get; set; }

    public int TrackId { get; set; }

    // Navigation
    [ForeignKey(nameof(PerformanceEventId))]
    public virtual PerformanceEvent PerformanceEvent { get; set; } = null!;

    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;
}
