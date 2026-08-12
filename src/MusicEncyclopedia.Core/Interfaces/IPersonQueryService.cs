using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides query operations for people.
/// </summary>
public interface IPersonQueryService
{
    /// <summary>
    /// Gets a paginated list of people for the specified culture.
    /// </summary>
    Task<PagedResult<NamedLinkDto>> GetPeopleAsync(
        string culture,
        int page = 1,
        int pageSize = 24,
        string? sort = null,
        string? q = null,
        string? personType = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the detail of a person by their slug.
    /// </summary>
    Task<object?> GetPersonBySlugAsync(
        string slug,
        string culture,
        CancellationToken cancellationToken = default);
}
