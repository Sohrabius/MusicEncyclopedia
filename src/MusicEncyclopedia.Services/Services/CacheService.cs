using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Provides caching operations backed by IMemoryCache.
/// Tracks all keys via <see cref="CacheKeyTracker"/> to enable pattern-based invalidation.
/// </summary>
public sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly CacheKeyTracker _keyTracker;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IMemoryCache memoryCache,
        CacheKeyTracker keyTracker,
        ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _keyTracker = keyTracker;
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
        {
            _logger.LogDebug("Cache HIT for key: {CacheKey}", key);
            return Task.FromResult<T?>(cachedValue);
        }

        _logger.LogDebug("Cache MISS for key: {CacheKey}", key);
        return Task.FromResult<T?>(null);
    }

    /// <inheritdoc />
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        var cacheOptions = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            cacheOptions
                .SetSlidingExpiration(expiration.Value)
                .SetAbsoluteExpiration(TimeSpan.FromHours(24)); // Max absolute cap
        }
        else
        {
            // Default: 10 minutes sliding, 1 hour absolute
            cacheOptions
                .SetSlidingExpiration(TimeSpan.FromMinutes(10))
                .SetAbsoluteExpiration(TimeSpan.FromHours(1));
        }

        _memoryCache.Set(key, value, cacheOptions);
        _keyTracker.Add(key);
        _logger.LogDebug("Cache SET for key: {CacheKey}, expiration: {Expiration}", key, expiration);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        _keyTracker.Remove(key);
        _logger.LogDebug("Cache REMOVED for key: {CacheKey}", key);

        return Task.CompletedTask;
    }
}
