using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Core.Interfaces;

/// <summary>
/// Provides credit-related query operations.
/// </summary>
public interface ICreditService
{
    /// <summary>
    /// Gets all credits for a specific entity.
    /// </summary>
    Task<IReadOnlyList<CreditDto>> GetCreditsAsync(
        int entityId,
        string entityTypeCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets credits grouped by role for a specific entity.
    /// </summary>
    Task<IReadOnlyList<CreditGroupDto>> GetGroupedCreditsAsync(
        int entityId,
        string entityTypeCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets musician credits (with instrument information) for a specific entity.
    /// </summary>
    Task<IReadOnlyList<MusicianCreditDto>> GetMusicianCreditsAsync(
        int entityId,
        string entityTypeCode,
        CancellationToken cancellationToken = default);
}
