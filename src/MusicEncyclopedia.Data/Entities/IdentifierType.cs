using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("IdentifierType")]
public class IdentifierType
{
    [Key]
    public int IdentifierTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<AlbumIdentifier> AlbumIdentifiers { get; set; } = new List<AlbumIdentifier>();
}
