namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the album tracklist editor (spec 10.5 "Tracklist" tab).
/// </summary>
public sealed class AlbumTracklistViewModel
{
    public int AlbumId { get; init; }

    /// <summary>Rows ordered by disc number, then sequence number.</summary>
    public IReadOnlyList<AlbumTrackRow> Rows { get; init; } = [];

    /// <summary>Candidate tracks for the "add existing track" dropdown.</summary>
    public IReadOnlyList<AlbumTrackOption> AvailableTracks { get; init; } = [];
}

/// <summary>A single tracklist row.</summary>
public sealed class AlbumTrackRow
{
    public int AlbumTrackId { get; init; }
    public int TrackId { get; init; }
    public string TrackTitle { get; init; } = "";
    public int DiscNumber { get; init; }
    public int TrackNumber { get; init; }
    public int SequenceNumber { get; init; }
    public string? TitleOverride { get; init; }
    public int? DurationOverrideSeconds { get; init; }
    public bool IsBonus { get; init; }
    public bool IsHidden { get; init; }
}

/// <summary>A candidate track for adding to the album.</summary>
public sealed class AlbumTrackOption
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
}
