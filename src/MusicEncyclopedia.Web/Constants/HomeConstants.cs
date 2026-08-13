namespace MusicEncyclopedia.Web.Constants;

/// <summary>
/// Single source of truth for the home page album catalog behavior
/// (see HOME_REDESIGN_PLAN.md — locked decisions #1 and #2).
/// </summary>
public static class HomeConstants
{
    /// <summary>Albums rendered on the first page (and appended per "Load more" click).</summary>
    public const int InitialPageSize = 20;

    /// <summary>Total albums lazy-loaded before numbered pagination takes over.</summary>
    public const int LazyLoadCap = 100;

    /// <summary>Page size used by the numbered pagination once the cap is reached.</summary>
    public const int PaginationPageSize = 20;

    /// <summary>Number of "Load more" pages within the lazy zone (100 ÷ 20).</summary>
    public static int LazyLoadCapPages => LazyLoadCap / InitialPageSize;
}
