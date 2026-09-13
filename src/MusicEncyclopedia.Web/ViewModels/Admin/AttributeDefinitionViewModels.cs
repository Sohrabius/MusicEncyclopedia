using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

public sealed class AttributeDefinitionListViewModel
{
    public IReadOnlyList<AttributeDefinition> Items { get; init; } = [];
    public IReadOnlyList<EntityType> EntityTypes { get; init; } = [];
    public IReadOnlyList<string> DataTypes { get; init; } = [];
}

public sealed class AttributeDefinitionFormModel
{
    public int AttributeDefinitionId { get; set; }

    [Range(1, int.MaxValue)]
    public int EntityTypeId { get; set; }

    [Required, StringLength(255)]
    public string Name { get; set; } = "";

    [Required, StringLength(50)]
    public string DataTypeCode { get; set; } = "STRING";

    public bool IsRequired { get; set; }
}
