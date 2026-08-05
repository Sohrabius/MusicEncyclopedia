using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackVersionTypeAssignment")]
public class TrackVersionTypeAssignment
{
    [Key]
    public int TrackVersionTypeAssignmentId { get; set; }

    public int TrackId { get; set; }

    public int TrackVersionTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey(nameof(TrackVersionTypeId))]
    public virtual TrackVersionType TrackVersionType { get; set; } = null!;
}
