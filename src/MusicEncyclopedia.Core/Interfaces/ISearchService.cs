using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides full-text search operations across entities.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Executes a search query and returns paginated results.
    /// </summary>
    Task<PagedResult<SearchResultDto>> SearchAsync(
        SearchQuery query,
        CancellationToken cancellationToken = default);
}
