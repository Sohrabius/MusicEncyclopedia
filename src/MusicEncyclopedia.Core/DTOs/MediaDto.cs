namespace MusicEncyclopedia.Core.DTOs;

/// <summary>
/// Represents a media item (image, audio, video, document) attached to an entity.
/// </summary>
public sealed class MediaDto
{
    public int MediaId { get; init; }
    public string Url { get; init; } = "";
    public string? ThumbnailUrl { get; init; }
    public string MediaType { get; init; } = "";
    public string? MediaRole { get; init; }
    public string? Description { get; init; }
    public bool IsPrimary { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}
