namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// Represents a media item attached to an entity.
/// </summary>
public sealed class MediaDto
{
    public int MediaId { get; init; }
    public string FileName { get; init; } = "";
    public string FilePath { get; init; } = "";
    public string? Url { get; init; }
    public string? ThumbnailUrl150 { get; init; }
    public string? ThumbnailUrl300 { get; init; }
    public string? ThumbnailUrl600 { get; init; }
    public string MimeType { get; init; } = "";
    public bool IsPrimary { get; init; }
    public int DisplayOrder { get; init; }
    public string? MediaRoleName { get; init; }
}

/// <summary>
/// Represents a citation attached to an entity.
/// </summary>
public sealed class CitationDto
{
    public int CitationId { get; init; }
    public int EntityTypeId { get; init; }
    public int EntityId { get; init; }
    public string? FieldName { get; init; }
    public string? Quote { get; init; }
    public string? PageNumber { get; init; }
    public string? CitationUrl { get; init; }
    public DateOnly? AccessedDate { get; init; }
    public string? SourceTitle { get; init; }
    public string? SourceSlug { get; init; }

    // Computed helper for view
    public string? SourceName => SourceTitle;
}

/// <summary>
/// Represents a single credit for an entity.
/// </summary>
public sealed class CreditDto
{
    public int CreditId { get; init; }
    public int? PersonId { get; init; }
    public string? PersonFullName { get; init; }
    public string? PersonSlug { get; init; }
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public string? CompanySlug { get; init; }
    public string RoleName { get; init; } = "";
    public string? RoleCode { get; init; }
    public int RoleDisplayOrder { get; init; }
    public string? InstrumentName { get; init; }
    public int DisplayOrder { get; init; }
    public bool IsPrimary { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Represents an award assignment to an entity (winner/nominee).
/// </summary>
public sealed record AwardAssignmentDto
{
    public int AwardAssignmentId { get; init; }
    public int EntityTypeId { get; init; }
    public int EntityId { get; init; }
    public int? Year { get; init; }
    public string? Category { get; init; }
    public string? Notes { get; init; }
    public string? ResultTypeName { get; init; }
    public string? ResultTypeCode { get; init; }
    public string? EntityName { get; init; }
}
