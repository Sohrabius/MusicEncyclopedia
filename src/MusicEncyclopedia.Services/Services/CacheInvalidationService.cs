using Microsoft.Extensions.Logging;
using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Services.Services;

/// <summary>
/// Service responsible for invalidating cached data when entities are created,
/// updated, deleted, or restored. Works with <see cref="CacheKeyTracker"/> to
/// find and remove cache keys matching entity-related patterns.
/// </summary>
public sealed class CacheInvalidationService
{
    private readonly ICacheService _cacheService;
    private readonly CacheKeyTracker _keyTracker;
    private readonly ILogger<CacheInvalidationService> _logger;

    // Maps known EntityTypeId values to type codes used in cache key patterns.
    // These correspond to seed data in the EntityType table.
    private static readonly Dictionary<int, string> EntityTypeIdMap = new()
    {
        { 1, "Album" },
        { 2, "Track" },
        { 3, "Person" },
        { 4, "Company" },
        { 5, "Genre" },
        { 6, "Mood" },
        { 7, "Instrument" },
        { 8, "Poem" },
        { 9, "SungVersion" },
        { 11, "RecordingSession" },
        { 12, "PerformanceEvent" },
        { 13, "Location" },
        { 16, "Tag" },
        { 17, "Award" },
        { 19, "Chart" },
    };

    public CacheInvalidationService(
        ICacheService cacheService,
        CacheKeyTracker keyTracker,
        ILogger<CacheInvalidationService> logger)
    {
        _cacheService = cacheService;
        _keyTracker = keyTracker;
        _logger = logger;
    }

    /// <summary>
    /// Invalidates all cached data that may be affected by a change to an entity.
    /// </summary>
    /// <param name="entityTypeCode">The entity type code (e.g., "Album", "Track").</param>
    /// <param name="entityId">The ID of the entity that changed.</param>
    /// <param name="changeType">The type of change: "Created", "Updated", "Deleted", or "Restored".</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task InvalidateEntityAsync(
        string entityTypeCode,
        int entityId,
        string changeType,
        CancellationToken ct = default)
    {
        var type = entityTypeCode.ToLowerInvariant();
        var patterns = BuildInvalidationPatterns(type);

        var keysToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pattern in patterns)
        {
            foreach (var key in _keyTracker.GetKeysByPattern(pattern))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            await _cacheService.RemoveAsync(key, ct);
        }

        _logger.LogInformation(
            "Cache invalidated: {Count} keys removed for {EntityType}#{EntityId} ({ChangeType})",
            keysToRemove.Count, entityTypeCode, entityId, changeType);
    }

    /// <summary>
    /// Invalidates cache for an entity identified by its EntityTypeId (instead of type code).
    /// </summary>
    /// <param name="entityTypeId">The numeric entity type ID from the EntityType table.</param>
    /// <param name="entityId">The ID of the entity that changed.</param>
    /// <param name="changeType">The type of change.</param>
    /// <param name="ct">Cancellation token.</param>
    public Task InvalidateEntityAsync(
        int entityTypeId,
        int entityId,
        string changeType,
        CancellationToken ct = default)
    {
        var typeCode = EntityTypeIdMap.TryGetValue(entityTypeId, out var code)
            ? code
            : "Unknown";
        return InvalidateEntityAsync(typeCode, entityId, changeType, ct);
    }

    /// <summary>
    /// Invalidates broad caches (home page, search results) when entity type is unknown
    /// or the change affects many entity types.
    /// </summary>
    public async Task InvalidateBroadAsync(CancellationToken ct = default)
    {
        var patterns = new[] { "home:", "search:" };

        var keysToRemove = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pattern in patterns)
        {
            foreach (var key in _keyTracker.GetKeysByPattern(pattern))
            {
                keysToRemove.Add(key);
            }
        }

        foreach (var key in keysToRemove)
        {
            await _cacheService.RemoveAsync(key, ct);
        }

        _logger.LogInformation("Broad cache invalidated: {Count} keys removed", keysToRemove.Count);
    }

    /// <summary>
    /// Builds the list of cache key patterns to invalidate for a given entity type.
    /// </summary>
    private static List<string> BuildInvalidationPatterns(string type)
    {
        var patterns = new List<string>
        {
            // Detail pages for this entity type
            $"{type}:detail:",

            // List pages for this entity type
            $"{type}:list:",

            // Home page
            "home:",

            // Search results
            "search:",
        };

        // When core entities change, also invalidate cross-referencing entity caches
        if (type is "album" or "track" or "person" or "company" or "poem" or "sungversion")
        {
            patterns.Add("album:detail:");
            patterns.Add("track:detail:");
            patterns.Add("person:detail:");
            patterns.Add("company:detail:");
        }

        return patterns;
    }
}
