using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumRelation")]
public class AlbumRelation
{
    [Key]
    public int AlbumRelationId { get; set; }

    public int AlbumId { get; set; }

    public int RelatedAlbumId { get; set; }

    public int AlbumRelationTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(RelatedAlbumId))]
    public virtual Album RelatedAlbum { get; set; } = null!;

    [ForeignKey(nameof(AlbumRelationTypeId))]
    public virtual AlbumRelationType AlbumRelationType { get; set; } = null!;
}
