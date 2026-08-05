using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Genre")]
public class Genre
{
    [Key]
    public int GenreId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? NameSort { get; set; }

    public string? Description { get; set; }

    public int? ParentGenreId { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;

    [ForeignKey(nameof(ParentGenreId))]
    public virtual Genre? ParentGenre { get; set; }

    public virtual ICollection<Genre> ChildGenres { get; set; } = new List<Genre>();
    public virtual ICollection<AlbumGenre> AlbumGenres { get; set; } = new List<AlbumGenre>();
    public virtual ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
}
