using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("TrackGenre")]
public class TrackGenre
{
    [Key]
    public int TrackGenreId { get; set; }

    public int TrackId { get; set; }

    public int GenreId { get; set; }

    // Navigation
    [ForeignKey(nameof(TrackId))]
    public virtual Track Track { get; set; } = null!;

    [ForeignKey(nameof(GenreId))]
    public virtual Genre Genre { get; set; } = null!;
}
