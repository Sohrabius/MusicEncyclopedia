using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AttributeDefinition")]
public class AttributeDefinition
{
    [Key]
    public int AttributeDefinitionId { get; set; }

    public int EntityTypeId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string DataTypeCode { get; set; } = null!;

    public bool IsRequired { get; set; }

    // Navigation
    public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
