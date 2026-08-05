using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackSungVersion")]
public class TrackSungVersion
{
    [Key]
    public int TrackSungVersionId { get; set; }

    public int TrackId { get; set; }

    public int SungVersionId { get; set; }

    public int SequenceNumber { get; set; }

    public bool IsPrimary { get; set; }

    // Navigation
    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey(nameof(SungVersionId))]
    public virtual SungVersion SungVersion { get; set; } = null!;
}
