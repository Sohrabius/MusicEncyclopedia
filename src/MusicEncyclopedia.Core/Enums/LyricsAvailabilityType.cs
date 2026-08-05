namespace MusicEncyclopedia.Core.Enums;

/// <summary>
/// Determines the availability level of lyrics for a track.
/// </summary>
public enum LyricsAvailabilityType
{
    /// <summary>No lyrics available.</summary>
    None = 0,

    /// <summary>Lyrics are publicly visible to everyone.</summary>
    Public = 1,

    /// <summary>Lyrics are visible only to registered/authenticated users.</summary>
    Registered = 2,

    /// <summary>Lyrics require a request to access.</summary>
    Request = 3,

    /// <summary>Lyrics are restricted to users with special permission.</summary>
    Restricted = 4
}
