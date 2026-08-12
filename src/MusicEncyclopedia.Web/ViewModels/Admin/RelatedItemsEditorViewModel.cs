namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the generic AJAX related-items editor used by the
/// Album and Track "related editors" tabs (spec 10.5/10.6).
/// Each tab edits one assignment table (e.g. AlbumGenre, AlbumMood, TrackInstrument).
/// </summary>
public sealed class RelatedItemsEditorViewModel
{
    /// <summary>Identifies which assignment kind this editor operates on (e.g. "album-genres").</summary>
    public string Kind { get; init; } = "";

    /// <summary>The entity type id (Album = 1, Track = 2) owning the assignments.</summary>
    public int EntityTypeId { get; init; }

    /// <summary>The entity (album/track) id owning the assignments.</summary>
    public int EntityId { get; init; }

    /// <summary>Display title for the editor panel.</summary>
    public string Title { get; init; } = "";

    /// <summary>Currently assigned rows (id + display label).</summary>
    public IReadOnlyList<RelatedItemRow> Items { get; init; } = [];

    /// <summary>Candidate values for the "add" dropdown (option id + label).</summary>
    public IReadOnlyList<RelatedItemOption> Options { get; init; } = [];

    /// <summary>True when the add-dropdown is single-select (default); false renders free text input.</summary>
    public bool UsesDropdown { get; init; } = true;

    /// <summary>True when the assignment rows have an extra free-text field (e.g. catalog number).</summary>
    public bool HasExtraField { get; init; }

    /// <summary>Label for the extra free-text field.</summary>
    public string ExtraFieldLabel { get; init; } = "";

    /// <summary>Placeholder for the extra free-text field.</summary>
    public string ExtraFieldPlaceholder { get; init; } = "";

    /// <summary>True when the assignment rows have a role/type dropdown (e.g. company role type).</summary>
    public bool HasRoleField { get; init; }

    /// <summary>Label for the role dropdown.</summary>
    public string RoleFieldLabel { get; init; } = "";

    /// <summary>Role options rendered alongside the item (id + label).</summary>
    public IReadOnlyList<RelatedItemOption> RoleOptions { get; init; } = [];
}

/// <summary>A single assignment row in the editor.</summary>
public sealed class RelatedItemRow
{
    /// <summary>Primary key of the assignment row (e.g. AlbumGenreId).</summary>
    public int Id { get; init; }

    /// <summary>Display label (e.g. genre name).</summary>
    public string Label { get; init; } = "";

    /// <summary>Optional extra text (catalog number, ISRC, ...).</summary>
    public string? Extra { get; init; }

    /// <summary>Optional role/type id attached to this row.</summary>
    public int? RoleId { get; init; }

    /// <summary>Optional role label.</summary>
    public string? RoleLabel { get; init; }
}

/// <summary>A candidate option in the add dropdown.</summary>
public sealed class RelatedItemOption
{
    public int Id { get; init; }
    public string Label { get; init; } = "";
}
