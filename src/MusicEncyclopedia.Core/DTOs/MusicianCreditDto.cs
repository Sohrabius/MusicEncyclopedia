namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a musician credit that includes instrument information.
/// </summary>
public sealed class MusicianCreditDto
{
    public int CreditId { get; init; }
    public int PersonId { get; init; }
    public string PersonFullName { get; init; } = "";
    public string PersonSlug { get; init; } = "";
    public string InstrumentName { get; init; } = "";
    public int? InstrumentId { get; init; }
    public string? Notes { get; init; }
}
