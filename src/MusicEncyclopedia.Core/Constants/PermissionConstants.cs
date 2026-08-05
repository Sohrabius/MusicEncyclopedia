namespace MusicEncyclopedia.Core.Constants;

/// <summary>
/// Defines permission policy names used for authorization throughout the application.
/// </summary>
public static class PermissionConstants
{
    public const string CanManageAlbums = "CanManageAlbums";
    public const string CanManageTracks = "CanManageTracks";
    public const string CanManagePeople = "CanManagePeople";
    public const string CanManageCompanies = "CanManageCompanies";
    public const string CanManagePoems = "CanManagePoems";
    public const string CanManageGenres = "CanManageGenres";
    public const string CanManageMoods = "CanManageMoods";
    public const string CanManageInstruments = "CanManageInstruments";
    public const string CanManageSessions = "CanManageSessions";
    public const string CanManageEvents = "CanManageEvents";
    public const string CanManageLocations = "CanManageLocations";
    public const string CanManageAwards = "CanManageAwards";
    public const string CanManageCharts = "CanManageCharts";
    public const string CanManageMedia = "CanManageMedia";
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageLookupTables = "CanManageLookupTables";
    public const string CanManageCitations = "CanManageCitations";
    public const string CanManageLocalizations = "CanManageLocalizations";
    public const string CanPublishContent = "CanPublishContent";
    public const string CanDeleteContent = "CanDeleteContent";
    public const string CanViewRestrictedLyrics = "CanViewRestrictedLyrics";

    /// <summary>
    /// All permission policy names.
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        CanManageAlbums,
        CanManageTracks,
        CanManagePeople,
        CanManageCompanies,
        CanManagePoems,
        CanManageGenres,
        CanManageMoods,
        CanManageInstruments,
        CanManageSessions,
        CanManageEvents,
        CanManageLocations,
        CanManageAwards,
        CanManageCharts,
        CanManageMedia,
        CanManageUsers,
        CanManageLookupTables,
        CanManageCitations,
        CanManageLocalizations,
        CanPublishContent,
        CanDeleteContent,
        CanViewRestrictedLyrics
    };
}
