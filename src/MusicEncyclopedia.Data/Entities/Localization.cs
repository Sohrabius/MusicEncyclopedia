using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Localization")]
public class Localization
{
    [Key]
    public int LocalizationId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int? LanguageId { get; set; }

    [Required]
    [StringLength(255)]
    public string FieldName { get; set; } = null!;

    public string LocalizedText { get; set; } = null!;

    // Navigation
    [ForeignKey(nameof(LanguageId))]
    public virtual Language? Language { get; set; }
}
