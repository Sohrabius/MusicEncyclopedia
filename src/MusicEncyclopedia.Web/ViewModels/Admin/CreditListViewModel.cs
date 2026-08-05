using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the credit editor (spec 10.7).
/// Used for AJAX-based inline editing of credits on an entity.
/// </summary>
public sealed class CreditListViewModel
{
    /// <summary>The entity type ID that these credits belong to.</summary>
    public int EntityTypeId { get; init; }

    /// <summary>The entity ID that these credits belong to.</summary>
    public int EntityId { get; init; }

    /// <summary>List of credits for the entity.</summary>
    public IReadOnlyList<CreditRowViewModel> Credits { get; set; } = [];

    // Dropdown data
    public IReadOnlyList<CreditRole> CreditRoles { get; set; } = [];
    public IReadOnlyList<RoleScopeType> RoleScopeTypes { get; set; } = [];
    public IReadOnlyList<Person> People { get; set; } = [];
    public IReadOnlyList<Company> Companies { get; set; } = [];
    public IReadOnlyList<Instrument> Instruments { get; set; } = [];
}

/// <summary>
/// Represents a single credit row in the credit editor.
/// </summary>
public sealed class CreditRowViewModel
{
    public int CreditId { get; set; }

    public int CreditRoleId { get; set; }

    public int RoleScopeTypeId { get; set; }

    public int? PersonId { get; set; }

    public int? CompanyId { get; set; }

    public int? InstrumentId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsPrimary { get; set; }

    public string? Notes { get; set; }
}
