using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("MusicalKey")]
public class MusicalKey
{
    [Key]
    public int MusicalKeyId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<Track> Tracks { get; set; } = new List<Track>();
}
