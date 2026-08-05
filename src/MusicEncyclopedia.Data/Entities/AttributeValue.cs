using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AttributeValue")]
public class AttributeValue
{
    [Key]
    public int AttributeValueId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public int AttributeDefinitionId { get; set; }

    public int? LanguageId { get; set; }

    public string? ValueString { get; set; }

    public int? ValueInt { get; set; }

    [Column(TypeName = "decimal(18,6)")]
    public decimal? ValueDecimal { get; set; }

    public DateOnly? ValueDate { get; set; }

    public DateTime? ValueDateTime { get; set; }

    public bool? ValueBit { get; set; }

    public string? Notes { get; set; }

    // Navigation
    [ForeignKey(nameof(AttributeDefinitionId))]
    public virtual AttributeDefinition AttributeDefinition { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    public virtual Language? Language { get; set; }
}
