namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Generic paginated result wrapper.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>
    /// The items for the current page.
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int Page { get; init; }

    /// <summary>
    /// The number of items per page.
    /// </summary>
    public int PageSize { get; init; }

    /// <summary>
    /// The total number of items across all pages.
    /// </summary>
    public int TotalItems { get; init; }

    /// <summary>
    /// The total number of pages.
    /// </summary>
    public int TotalPages { get; init; }

    /// <summary>
    /// Creates a new <see cref="PagedResult{T}"/> with calculated total pages.
    /// </summary>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalItems)
    {
        return new PagedResult<T>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems > 0
                ? (int)Math.Ceiling(totalItems / (double)pageSize)
                : 0
        };
    }
}
