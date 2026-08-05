using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Track")]
public class Track
{
    [Key]
    public int TrackId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    [StringLength(500)]
    public string? TitleSort { get; set; }

    [StringLength(500)]
    public string? OriginalTitle { get; set; }

    [StringLength(500)]
    public string? EnglishTitle { get; set; }

    public int? DurationSeconds { get; set; }

    public DateOnly? RecordingStartDate { get; set; }

    public DateOnly? RecordingEndDate { get; set; }

    [StringLength(50)]
    public string? RecordingDatePrecision { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    [StringLength(50)]
    public string? ReleaseDatePrecision { get; set; }

    public string? Description { get; set; }

    public int? LyricsAvailabilityTypeId { get; set; }

    public int? VocalStyleId { get; set; }

    public int? MusicalKeyId { get; set; }

    public short? BPM { get; set; }

    [StringLength(12)]
    public string? ISRC { get; set; }

    public bool IsInstrumental { get; set; }

    public bool IsExplicit { get; set; }

    [StringLength(1000)]
    public string? CopyrightNotice { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public bool IsDeleted { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [StringLength(255)]
    public string? ModifiedBy { get; set; }

    public DateTime? ModifiedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(EntityId))]
    public virtual Entity Entity { get; set; } = null!;

    [ForeignKey(nameof(LyricsAvailabilityTypeId))]
    public virtual LyricsAvailabilityType? LyricsAvailabilityType { get; set; }

    [ForeignKey(nameof(VocalStyleId))]
    public virtual VocalStyle? VocalStyle { get; set; }

    [ForeignKey(nameof(MusicalKeyId))]
    public virtual MusicalKey? MusicalKey { get; set; }

    public virtual ICollection<AlbumTrack> AlbumTracks { get; set; } = new List<AlbumTrack>();
    public virtual ICollection<TrackGenre> TrackGenres { get; set; } = new List<TrackGenre>();
    public virtual ICollection<TrackMood> TrackMoods { get; set; } = new List<TrackMood>();
    public virtual ICollection<TrackInstrument> TrackInstruments { get; set; } = new List<TrackInstrument>();
    public virtual ICollection<TrackSungVersion> TrackSungVersions { get; set; } = new List<TrackSungVersion>();
    public virtual ICollection<RecordingSessionTrack> RecordingSessionTracks { get; set; } = new List<RecordingSessionTrack>();
    public virtual ICollection<PerformanceEventTrack> PerformanceEventTracks { get; set; } = new List<PerformanceEventTrack>();
    public virtual ICollection<TrackRelation> TrackRelations { get; set; } = new List<TrackRelation>();
    public virtual ICollection<TrackRelation> RelatedTrackRelations { get; set; } = new List<TrackRelation>();
    public virtual ICollection<TrackVersionTypeAssignment> TrackVersionTypeAssignments { get; set; } = new List<TrackVersionTypeAssignment>();
}
