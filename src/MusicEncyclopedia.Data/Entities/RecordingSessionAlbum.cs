using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("RecordingSessionAlbum")]
public class RecordingSessionAlbum
{
    [Key]
    public int RecordingSessionAlbumId { get; set; }

    public int RecordingSessionId { get; set; }

    public int AlbumId { get; set; }

    // Navigation
    [ForeignKey(nameof(RecordingSessionId))]
    public virtual RecordingSession RecordingSession { get; set; } = null!;

    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;
}
