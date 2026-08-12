using System.Security.Cryptography;
using System.Text;

namespace MusicEncyclopedia.Core.Infrastructure;

/// <summary>
/// Builds cache keys that follow the spec §15.1 pattern
/// (<c>{type}:{scope}:{culture}:{id}</c>) and match the key prefixes used by
/// <see cref="MusicEncyclopedia.Services.Services.CacheInvalidationService"/>
/// for pattern-based invalidation, plus the spec §15.2 cache durations.
/// </summary>
public static class CacheKeys
{
    // Spec §15.2 cache durations.
    public static readonly TimeSpan HomeDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan ListDuration = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan DetailDuration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan SearchDuration = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan LookupDuration = TimeSpan.FromHours(1);

    /// <summary>Key: <c>home:{culture}</c></summary>
    public static string Home(string culture) => $"home:{culture}";

    /// <summary>Key: <c>{entityType}:detail:{culture}:{slug}</c></summary>
    public static string Detail(string entityType, string culture, string slug) =>
        $"{entityType}:detail:{culture}:{slug}";

    /// <summary>Key: <c>{entityType}:list:{culture}:{hash}</c> — hash of all filter values.</summary>
    public static string List(string entityType, string culture, params object?[] filterValues) =>
        $"{entityType}:list:{culture}:{StableHash(filterValues)}";

    /// <summary>Key: <c>search:{culture}:{hash}</c> — hash of the full search query.</summary>
    public static string Search(string culture, params object?[] queryValues) =>
        $"search:{culture}:{StableHash(queryValues)}";

    /// <summary>Key: <c>lookup:{name}</c> — lookup lists that rarely change.</summary>
    public static string Lookup(string name) => $"lookup:{name}";

    /// <summary>
    /// Deterministic, process-stable hash of filter values so cache keys are stable
    /// across restarts (unlike <see cref="HashCode"/>) yet stay compact.
    /// </summary>
    private static string StableHash(object?[] values)
    {
        var text = string.Join('\u001F', values.Select(v => v?.ToString() ?? string.Empty));
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }
}
