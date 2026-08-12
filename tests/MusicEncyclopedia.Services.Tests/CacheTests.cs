using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using MusicEncyclopedia.Core.Infrastructure;
using MusicEncyclopedia.Services.Services;

namespace MusicEncyclopedia.Services.Tests;

/// <summary>
/// Verifies the spec §15 cache layer: key stability, §15.2 durations, and
/// pattern-based invalidation (including the fixed EntityTypeId map).
/// </summary>
public class CacheTests
{
    private static (CacheService Cache, CacheInvalidationService Invalidation) CreateServices()
    {
        var tracker = new CacheKeyTracker();
        var cache = new CacheService(
            new MemoryCache(new MemoryCacheOptions()),
            tracker,
            NullLogger<CacheService>.Instance);
        var invalidation = new CacheInvalidationService(
            cache,
            tracker,
            NullLogger<CacheInvalidationService>.Instance);
        return (cache, invalidation);
    }

    [Fact]
    public async Task InvalidateEntityAsync_RemovesDetailAndListKeysForEntityType()
    {
        var (cache, invalidation) = CreateServices();

        var detailKey = CacheKeys.Detail("album", "en", "midnight-garden");
        var listKey = CacheKeys.List("album", "en", 1, 24, null, null, null, null, null);
        await cache.SetAsync(detailKey, "detail");
        await cache.SetAsync(listKey, "list");

        await invalidation.InvalidateEntityAsync("Album", 1, "Updated");

        (await cache.GetAsync<string>(detailKey)).Should().BeNull();
        (await cache.GetAsync<string>(listKey)).Should().BeNull();
    }

    [Fact]
    public async Task InvalidateEntityAsync_ByEntityTypeId_UsesFixedMap()
    {
        var (cache, invalidation) = CreateServices();

        // EntityType seed ids: 14 = Award, 16 = Chart, 18 = Tag,
        // 10 = Publication, 15 = Certification, 17 = Source.
        var awardKey = CacheKeys.Detail("award", "en", "award-1");
        var chartKey = CacheKeys.Detail("chart", "en", "chart-1");
        var tagKey = CacheKeys.Detail("tag", "en", "tag-1");
        var pubKey = CacheKeys.Detail("publication", "en", "pub-1");
        var certKey = CacheKeys.Detail("certification", "en", "cert-1");
        var sourceKey = CacheKeys.Detail("source", "en", "src-1");

        foreach (var key in new[] { awardKey, chartKey, tagKey, pubKey, certKey, sourceKey })
        {
            await cache.SetAsync(key, "value");
        }

        await invalidation.InvalidateEntityAsync(14, 1, "Updated");
        await invalidation.InvalidateEntityAsync(16, 1, "Updated");
        await invalidation.InvalidateEntityAsync(18, 1, "Updated");
        await invalidation.InvalidateEntityAsync(10, 1, "Updated");
        await invalidation.InvalidateEntityAsync(15, 1, "Updated");
        await invalidation.InvalidateEntityAsync(17, 1, "Updated");

        foreach (var key in new[] { awardKey, chartKey, tagKey, pubKey, certKey, sourceKey })
        {
            (await cache.GetAsync<string>(key)).Should().BeNull($"key {key} should be invalidated");
        }
    }

    [Fact]
    public async Task InvalidateEntityAsync_RemovesCrossReferencingDetailKeys()
    {
        var (cache, invalidation) = CreateServices();

        var tagDetail = CacheKeys.Detail("tag", "en", "jazz");
        var albumDetail = CacheKeys.Detail("album", "en", "midnight-garden");
        var personDetail = CacheKeys.Detail("person", "en", "niloofar-rahimi");
        await cache.SetAsync(tagDetail, "tag");
        await cache.SetAsync(albumDetail, "album");
        await cache.SetAsync(personDetail, "person");

        // A Tag change must refresh album/person detail pages that render tags.
        await invalidation.InvalidateEntityAsync("Tag", 5, "Updated");

        (await cache.GetAsync<string>(albumDetail)).Should().BeNull();
        (await cache.GetAsync<string>(personDetail)).Should().BeNull();
    }

    [Fact]
    public async Task InvalidateBroadAsync_RemovesHomeSearchAndLookupKeys()
    {
        var (cache, invalidation) = CreateServices();

        await cache.SetAsync(CacheKeys.Home("fa"), "home");
        await cache.SetAsync(CacheKeys.Search("en", "garden", null, null, null, null, 1, 20), "search");
        await cache.SetAsync(CacheKeys.Lookup("genre"), "lookup");

        await invalidation.InvalidateBroadAsync();

        (await cache.GetAsync<string>(CacheKeys.Home("fa"))).Should().BeNull();
        (await cache.GetAsync<string>(CacheKeys.Search("en", "garden", null, null, null, null, 1, 20))).Should().BeNull();
        (await cache.GetAsync<string>(CacheKeys.Lookup("genre"))).Should().BeNull();
    }

    [Fact]
    public void CacheKeys_AreStableAcrossInstances()
    {
        var key1 = CacheKeys.List("album", "en", 1, 24, null, "rock", null, null, "garden");
        var key2 = CacheKeys.List("album", "en", 1, 24, null, "rock", null, null, "garden");

        key1.Should().Be(key2);
        key1.Should().StartWith("album:list:en:");
    }

    [Fact]
    public void CacheKeys_Durations_MatchSpec15_2()
    {
        CacheKeys.HomeDuration.Should().Be(TimeSpan.FromMinutes(5));
        CacheKeys.ListDuration.Should().Be(TimeSpan.FromMinutes(5));
        CacheKeys.DetailDuration.Should().Be(TimeSpan.FromMinutes(10));
        CacheKeys.SearchDuration.Should().Be(TimeSpan.FromMinutes(1));
    }
}
