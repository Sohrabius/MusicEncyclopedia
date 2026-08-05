namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class LocationDetailViewModel
{
    public required LocationDetailDto Location { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed record LocationDetailDto
{
    public int LocationId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? LocationTypeName { get; init; }
    public string? CountryName { get; init; }
    public decimal? Latitude { get; init; }
    public decimal? Longitude { get; init; }

    // Parent location
    public int? ParentLocationId { get; init; }
    public string? ParentLocationName { get; init; }
    public string? ParentLocationSlug { get; init; }

    // Sub-locations (children)
    public IReadOnlyList<LocationChildDto> Children { get; init; } = [];

    // Sessions at this location
    public IReadOnlyList<LocationSessionDto> Sessions { get; init; } = [];

    // Events at this location
    public IReadOnlyList<LocationEventDto> Events { get; init; } = [];

    // People born/died here
    public IReadOnlyList<LocationPersonDto> PeopleBornHere { get; init; } = [];
    public IReadOnlyList<LocationPersonDto> PeopleDiedHere { get; init; } = [];

    // Media
    public IReadOnlyList<MediaDto> Media { get; init; } = [];
}

public sealed record LocationChildDto
{
    public int LocationId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? LocationTypeName { get; init; }
}

public sealed record LocationSessionDto
{
    public int RecordingSessionId { get; init; }
    public string Slug { get; init; } = "";
    public string? SessionTypeName { get; init; }
    public DateOnly? StartDate { get; init; }
}

public sealed record LocationEventDto
{
    public int PerformanceEventId { get; init; }
    public string Slug { get; init; } = "";
    public string? EventTypeName { get; init; }
    public DateTime? Date { get; init; }
}

public sealed record LocationPersonDto
{
    public int PersonId { get; init; }
    public string FullName { get; init; } = "";
    public string Slug { get; init; } = "";
}
