using MusicEncyclopedia.Core.DTOs;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the admin dashboard page (spec 10.4).
/// Provides aggregate statistics and recent activity summaries.
/// </summary>
public sealed class AdminDashboardViewModel
{
    /// <summary>Total number of non-deleted albums.</summary>
    public int AlbumCount { get; init; }

    /// <summary>Total number of non-deleted tracks.</summary>
    public int TrackCount { get; init; }

    /// <summary>Total number of non-deleted people.</summary>
    public int PersonCount { get; init; }

    /// <summary>Total number of non-deleted companies.</summary>
    public int CompanyCount { get; init; }

    /// <summary>Total number of non-deleted poems.</summary>
    public int PoemCount { get; init; }

    /// <summary>Recently created or modified albums (up to 10).</summary>
    public IReadOnlyList<AlbumListItemDto> RecentAlbums { get; init; } = [];

    /// <summary>Recently created or modified tracks (up to 10).</summary>
    public IReadOnlyList<TrackListItemDto> RecentTracks { get; init; } = [];
}

/// <summary>
/// Lightweight DTO for displaying a track in the admin dashboard recent list.
/// </summary>
public sealed class TrackListItemDto
{
    public int TrackId { get; init; }
    public string Title { get; init; } = "";
    public string Slug { get; init; } = "";
    public string? OriginalTitle { get; init; }
    public string? EnglishTitle { get; init; }
    public string? AlbumTitle { get; init; }
    public int? DurationSeconds { get; init; }
}
