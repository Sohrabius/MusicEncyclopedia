using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumTrack")]
public class AlbumTrack
{
    [Key]
    public int AlbumTrackId { get; set; }

    public int AlbumId { get; set; }

    public int TrackId { get; set; }

    public int DiscNumber { get; set; }

    public int TrackNumber { get; set; }

    public int SequenceNumber { get; set; }

    [StringLength(500)]
    public string? TrackTitleOverride { get; set; }

    public int? DurationSecondsOverride { get; set; }

    public bool IsBonus { get; set; }

    public bool IsHidden { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;
}
