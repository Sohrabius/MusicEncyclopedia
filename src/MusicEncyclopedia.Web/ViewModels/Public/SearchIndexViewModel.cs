using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the search page (spec 9.21).
/// Route: /{culture}/search
/// </summary>
public sealed class SearchIndexViewModel
{
    public string? Query { get; init; }
    public string? EntityType { get; init; }
    public string? Genre { get; init; }
    public string? Mood { get; init; }
    public string? Instrument { get; init; }
    public PagedResult<SearchResultDto>? Results { get; init; }
    public string Culture { get; init; } = "fa";

    public static readonly string[] KnownEntityTypes =
        ["Album", "Track", "Person", "Poem", "SungVersion", "Company", "Genre", "Mood", "Instrument"];

    public List<LookupItem> Genres { get; init; } = [];
    public List<LookupItem> Moods { get; init; } = [];
    public List<LookupItem> Instruments { get; init; } = [];
}

public sealed class LookupItem
{
    public string Slug { get; init; } = "";
    public string Name { get; init; } = "";
}
