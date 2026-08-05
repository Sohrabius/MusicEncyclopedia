using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackMood")]
public class TrackMood
{
    [Key]
    public int TrackMoodId { get; set; }

    public int TrackId { get; set; }

    public int MoodId { get; set; }

    // Navigation
    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey(nameof(MoodId))]
    public virtual Mood Mood { get; set; } = null!;
}
