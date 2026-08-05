using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("EntityLink")]
public class EntityLink
{
    [Key]
    public int EntityLinkId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int? LinkTypeId { get; set; }

    [Required]
    [StringLength(2000)]
    public string Url { get; set; } = null!;

    [StringLength(500)]
    public string? Title { get; set; }

    public bool IsDeleted { get; set; }

    // Navigation
    [ForeignKey(nameof(LinkTypeId))]
    public virtual LinkType? LinkType { get; set; }
}
