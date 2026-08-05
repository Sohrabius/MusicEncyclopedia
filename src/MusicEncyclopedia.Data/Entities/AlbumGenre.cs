using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumGenre")]
public class AlbumGenre
{
    [Key]
    public int AlbumGenreId { get; set; }

    public int AlbumId { get; set; }

    public int GenreId { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(GenreId))]
    public virtual Genre Genre { get; set; } = null!;
}
