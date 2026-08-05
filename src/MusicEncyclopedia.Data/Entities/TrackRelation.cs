using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackRelation")]
public class TrackRelation
{
    [Key]
    public int TrackRelationId { get; set; }

    public int TrackId { get; set; }

    public int RelatedTrackId { get; set; }

    public int TrackRelationTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey(nameof(RelatedTrackId))]
    public virtual Track RelatedTrack { get; set; } = null!;

    [ForeignKey(nameof(TrackRelationTypeId))]
    public virtual TrackRelationType TrackRelationType { get; set; } = null!;
}
