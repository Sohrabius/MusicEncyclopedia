using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumMood")]
public class AlbumMood
{
    [Key]
    public int AlbumMoodId { get; set; }

    public int AlbumId { get; set; }

    public int MoodId { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(MoodId))]
    public virtual Mood Mood { get; set; } = null!;
}
