using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("RecordingSessionTrack")]
public class RecordingSessionTrack
{
    [Key]
    public int RecordingSessionTrackId { get; set; }

    public int RecordingSessionId { get; set; }

    public int TrackId { get; set; }

    // Navigation
    [ForeignKey(nameof(RecordingSessionId))]
    public virtual RecordingSession RecordingSession { get; set; } = null!;

    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;
}
