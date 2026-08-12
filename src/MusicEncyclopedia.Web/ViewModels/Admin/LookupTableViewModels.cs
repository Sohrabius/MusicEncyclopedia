namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>View model for the lookup tables index page.</summary>
public sealed class LookupTableListViewModel
{
    public IReadOnlyList<LookupTableSummary> Tables { get; init; } = [];
}

/// <summary>Summary of one lookup table on the index page.</summary>
public sealed class LookupTableSummary
{
    public string Key { get; init; } = "";
    public string Label { get; init; } = "";
    public string EntityName { get; init; } = "";
    public int RowCount { get; init; }
}

/// <summary>View model for a single lookup table's items.</summary>
public sealed class LookupTableItemsViewModel
{
    public string Key { get; init; } = "";
    public string Label { get; init; } = "";
    public string EntityName { get; init; } = "";
    public IReadOnlyList<LookupItemRow> Items { get; init; } = [];
    public string? Message { get; init; }
    public bool MessageIsError { get; init; }
}

/// <summary>A row in a lookup table.</summary>
public sealed class LookupItemRow
{
    public int Id { get; init; }
    public string? Code { get; init; }
    public string? Name { get; init; }
    public string? Extra { get; init; }
    public string DisplayName => !string.IsNullOrWhiteSpace(Name)
        ? Name
        : (string.IsNullOrWhiteSpace(Code) ? $"#{Id}" : Code);
}

/// <summary>Form model for creating/editing a lookup row.</summary>
public sealed class LookupItemFormModel
{
    public string Key { get; set; } = "";
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? NameSort { get; set; }
    public string? Description { get; set; }
}
