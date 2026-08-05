namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a company associated with an album in a specific role.
/// </summary>
public sealed class AlbumCompanyDto
{
    public int AlbumCompanyId { get; init; }
    public int CompanyId { get; init; }
    public string CompanyName { get; init; } = "";
    public string CompanySlug { get; init; } = "";
    public string CompanyRole { get; init; } = "";
    public string? CatalogNumber { get; init; }
    public string? Barcode { get; init; }
}
