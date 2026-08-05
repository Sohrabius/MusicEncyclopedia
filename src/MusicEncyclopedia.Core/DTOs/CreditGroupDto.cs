namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Groups credits under a common role name for display purposes.
/// </summary>
public sealed class CreditGroupDto
{
    public string RoleName { get; init; } = "";
    public IReadOnlyList<CreditDto> Credits { get; init; } = [];
}
