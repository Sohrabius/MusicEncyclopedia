using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Alias")]
public class Alias
{
    [Key]
    public int AliasId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int? AliasTypeId { get; set; }

    public int? LanguageId { get; set; }

    [Required]
    [StringLength(500)]
    public string AliasName { get; set; } = null!;

    public bool IsPrimary { get; set; }

    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(AliasTypeId))]
    public virtual AliasType? AliasType { get; set; }

    [ForeignKey(nameof(LanguageId))]
    public virtual Language? Language { get; set; }
}
