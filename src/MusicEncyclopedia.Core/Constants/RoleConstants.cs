namespace MusicEncyclopedia.Core.Constants;

/// <summary>
/// Defines the admin role names used in the application.
/// </summary>
public static class RoleConstants
{
    public const string Administrator = "Administrator";
    public const string Editor = "Editor";
    public const string Contributor = "Contributor";
    public const string Reviewer = "Reviewer";
    public const string MediaManager = "MediaManager";
    public const string UserManager = "UserManager";

    /// <summary>
    /// All admin role names.
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        Administrator,
        Editor,
        Contributor,
        Reviewer,
        MediaManager,
        UserManager
    };
}
