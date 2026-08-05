using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the attribute value editor (spec 10.15).
/// Lists attribute values for a given entity.
/// </summary>
public sealed class AttributeValueListViewModel
{
    public int EntityTypeId { get; init; }
    public int EntityId { get; init; }
    public IReadOnlyList<AttributeValueRowViewModel> AttributeValues { get; set; } = [];

    public IReadOnlyList<AttributeDefinition> AttributeDefinitions { get; set; } = [];
    public IReadOnlyList<Language> Languages { get; set; } = [];
}

/// <summary>
/// Represents a single attribute value row.
/// </summary>
public sealed class AttributeValueRowViewModel
{
    public int AttributeValueId { get; set; }
    public int AttributeDefinitionId { get; set; }
    public int? LanguageId { get; set; }
    public string? ValueString { get; set; }
    public int? ValueInt { get; set; }
    public decimal? ValueDecimal { get; set; }
    public DateOnly? ValueDate { get; set; }
    public DateTime? ValueDateTime { get; set; }
    public bool? ValueBit { get; set; }
    public string? Notes { get; set; }
}
