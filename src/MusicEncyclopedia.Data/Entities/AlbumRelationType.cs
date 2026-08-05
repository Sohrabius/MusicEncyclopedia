using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumRelationType")]
public class AlbumRelationType
{
    [Key]
    public int AlbumRelationTypeId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<AlbumRelation> AlbumRelations { get; set; } = new List<AlbumRelation>();
}
