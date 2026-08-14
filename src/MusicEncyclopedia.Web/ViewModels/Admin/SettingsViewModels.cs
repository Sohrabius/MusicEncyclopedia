namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin settings page — a read-only overview of the
/// application configuration and runtime environment.
/// </summary>
public sealed class SettingsViewModel
{
    public string Environment { get; init; } = "";
    public string RuntimeVersion { get; init; } = "";
    public string DatabaseProvider { get; init; } = "";
    public string DefaultCulture { get; init; } = "";
    public string BaseCulture { get; init; } = "";
    public string FallbackCulture { get; init; } = "";
    public IReadOnlyList<string> SupportedCultures { get; init; } = [];
    public string? SiteBaseUrl { get; init; }
    public int DefaultPublicPageCacheMinutes { get; init; }
    public int DefaultPageSize { get; init; }
    public int MaxLoginAttempts { get; init; }
    public string MediaStoragePath { get; init; } = "";
    public string SearchProvider { get; init; } = "";
}
