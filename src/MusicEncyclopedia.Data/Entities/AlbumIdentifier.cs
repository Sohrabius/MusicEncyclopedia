using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumIdentifier")]
public class AlbumIdentifier
{
    [Key]
    public int AlbumIdentifierId { get; set; }

    public int AlbumId { get; set; }

    public int? IdentifierTypeId { get; set; }

    [Required]
    [StringLength(255)]
    public string Value { get; set; } = null!;

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(IdentifierTypeId))]
    public virtual IdentifierType? IdentifierType { get; set; }
}
