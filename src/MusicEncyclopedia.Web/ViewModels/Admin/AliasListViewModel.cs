using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the alias editor (spec 10.13).
/// Lists aliases for a given entity with inline editing.
/// </summary>
public sealed class AliasListViewModel
{
    public int EntityTypeId { get; init; }
    public int EntityId { get; init; }
    public IReadOnlyList<AliasRowViewModel> Aliases { get; set; } = [];

    public IReadOnlyList<AliasType> AliasTypes { get; set; } = [];
    public IReadOnlyList<Language> Languages { get; set; } = [];
}

/// <summary>
/// Represents a single alias row.
/// </summary>
public sealed class AliasRowViewModel
{
    public int AliasId { get; set; }
    public int? AliasTypeId { get; set; }
    public int? LanguageId { get; set; }
    public string AliasName { get; set; } = "";
    public bool IsPrimary { get; set; }
    public string? Notes { get; set; }
}
