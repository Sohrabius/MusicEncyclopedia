using MusicEncyclopedia.Core.Interfaces;

namespace MusicEncyclopedia.Web.Jobs;

/// <summary>
/// Warms the public list caches (spec §20 background jobs, §15 caching) so the
/// most-visited pages are already in memory after a deployment or nightly restart.
/// Runs as a Hangfire recurring job, SQL Server only.
/// </summary>
public sealed class CacheWarmJob
{
    private static readonly string[] Cultures = ["en", "fa", "ar", "fr"];

    private readonly IAlbumQueryService _albums;
    private readonly ITrackQueryService _tracks;
    private readonly IPersonQueryService _people;
    private readonly ICompanyQueryService _companies;
    private readonly ILogger<CacheWarmJob> _logger;

    public CacheWarmJob(
        IAlbumQueryService albums,
        ITrackQueryService tracks,
        IPersonQueryService people,
        ICompanyQueryService companies,
        ILogger<CacheWarmJob> logger)
    {
        _albums = albums;
        _tracks = tracks;
        _people = people;
        _companies = companies;
        _logger = logger;
    }

    /// <summary>
    /// Warms the album/track/person/company list caches for every supported culture.
    /// </summary>
    public async Task WarmAsync()
    {
        var token = CancellationToken.None;

        foreach (var culture in Cultures)
        {
            await _albums.GetAlbumsAsync(culture, cancellationToken: token);
            await _tracks.GetTracksAsync(culture, cancellationToken: token);
            await _people.GetPeopleAsync(culture, cancellationToken: token);
            await _companies.GetCompaniesAsync(culture, cancellationToken: token);
        }

        _logger.LogInformation(
            "Cache warm-up completed for {CultureCount} cultures",
            Cultures.Length);
    }
}
