namespace MusicEncyclopedia.Web.ViewModels.Public;

/// <summary>
/// View model for the company detail page (9.7).
/// The Company property is dynamic because the service returns object?
/// (the concrete CompanyDetailDto lives in the services layer).
/// </summary>
public sealed class CompanyDetailViewModel
{
    /// <summary>
    /// The full company detail data from the service.
    /// Properties expected: Name, LogoUrl, CompanyType, Country, Website,
    /// History, AlbumsAsLabel, AlbumsAsPublisher, AlbumsAsDistributor,
    /// AlbumsAsProduction, AlbumsAsStudio, TrackCredits, Publications,
    /// Media, Links, Citations
    /// </summary>
    public required dynamic Company { get; init; }

    /// <summary>
    /// The current culture for URL generation.
    /// </summary>
    public string Culture { get; init; } = "fa";
}
