namespace MusicEncyclopedia.Web.ViewModels.Public;

public sealed class AwardDetailViewModel
{
    public required AwardDetailDto Award { get; init; }
    public string Culture { get; init; } = "fa";
}

public sealed record AwardDetailDto
{
    public int AwardId { get; init; }
    public string Name { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? Organization { get; init; }
    public string? CountryName { get; init; }
    public string? Description { get; init; }

    // Winners & Nominees
    public IReadOnlyList<AwardAssignmentDto> Assignments { get; init; } = [];
}
