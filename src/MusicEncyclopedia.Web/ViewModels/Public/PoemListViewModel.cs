namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the poem listing page.
/// Spec 8.1 (routes), 9.12 (poem detail referenced from listing).
/// </summary>
public sealed class PoemListViewModel
{
    /// <summary>
    /// Paginated list of poem items.
    /// </summary>
    public required PagedResultViewModel Items { get; init; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int Page { get; init; } = 1;

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";

    /// <summary>
    /// A single poem row returned from Dapper.
    /// </summary>
    public sealed class PoemRow
    {
        public string Slug { get; init; } = "";
        public string Title { get; init; } = "";
        public string? PoetName { get; init; }
        public string? PoetSlug { get; init; }
        public string? PublicationName { get; init; }
        public string? PublicationSlug { get; init; }
    }

    /// <summary>
    /// Pagination wrapper for poem rows.
    /// </summary>
    public sealed class PagedResultViewModel
    {
        public IReadOnlyList<PoemRow> Items { get; init; } = [];
        public int Page { get; init; }
        public int PageSize { get; init; }
        public int TotalItems { get; init; }
        public int TotalPages { get; init; }
    }
}
