namespace MusicEncyclopedia.Core.Constants;

/// <summary>
/// Entity type codes used throughout the application to identify entity kinds.
/// </summary>
public static class EntityTypeConstants
{
    public const string Album = "Album";
    public const string Track = "Track";
    public const string Person = "Person";
    public const string Company = "Company";
    public const string Genre = "Genre";
    public const string Mood = "Mood";
    public const string Instrument = "Instrument";
    public const string Poem = "Poem";
    public const string SungVersion = "SungVersion";
    public const string Publication = "Publication";
    public const string RecordingSession = "RecordingSession";
    public const string PerformanceEvent = "PerformanceEvent";
    public const string Location = "Location";
    public const string Award = "Award";
    public const string Certification = "Certification";
    public const string Chart = "Chart";
    public const string Source = "Source";
    public const string Tag = "Tag";

    /// <summary>
    /// All entity type codes.
    /// </summary>
    public static readonly IReadOnlyList<string> All = new[]
    {
        Album,
        Track,
        Person,
        Company,
        Genre,
        Mood,
        Instrument,
        Poem,
        SungVersion,
        Publication,
        RecordingSession,
        PerformanceEvent,
        Location,
        Award,
        Certification,
        Chart,
        Source,
        Tag
    };
}
