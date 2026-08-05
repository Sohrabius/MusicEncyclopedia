using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides query operations for companies.
/// </summary>
public interface ICompanyQueryService
{
    /// <summary>
    /// Gets a paginated list of companies for the specified culture.
    /// </summary>
    Task<PagedResult<NamedLinkDto>> GetCompaniesAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? q = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the detail of a company by its slug.
    /// </summary>
    Task<object?> GetCompanyBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default);
}
