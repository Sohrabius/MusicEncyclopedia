using System.Collections.Concurrent;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Singleton that tracks all cache keys set through <see cref="CacheService"/>.
/// Enables pattern-based cache invalidation by storing keys as they are created.
/// </summary>
public sealed class CacheKeyTracker
{
    private readonly ConcurrentDictionary<string, byte> _keys = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Registers a key in the tracker.
    /// </summary>
    public void Add(string key) => _keys.TryAdd(key, 0);

    /// <summary>
    /// Removes a key from the tracker.
    /// </summary>
    public void Remove(string key) => _keys.TryRemove(key, out _);

    /// <summary>
    /// Gets all tracked keys.
    /// </summary>
    public IEnumerable<string> GetKeys() => _keys.Keys;

    /// <summary>
    /// Gets all tracked keys that start with the given pattern.
    /// Used by <see cref="CacheInvalidationService"/> to find keys to invalidate.
    /// </summary>
    public IEnumerable<string> GetKeysByPattern(string pattern) =>
        _keys.Keys.Where(k => k.StartsWith(pattern, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Clears all tracked keys.
    /// </summary>
    public void Clear() => _keys.Clear();
}
