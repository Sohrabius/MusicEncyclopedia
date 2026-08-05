using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("RecordingSession")]
public class RecordingSession
{
    [Key]
    public int RecordingSessionId { get; set; }

    public int EntityId { get; set; }

    public int? SessionTypeId { get; set; }

    public int? LocationId { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    [StringLength(50)]
    public string? DatePrecision { get; set; }

    public string? Notes { get; set; }

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

    [ForeignKey(nameof(SessionTypeId))]
    public virtual SessionType? SessionType { get; set; }

    [ForeignKey(nameof(LocationId))]
    public virtual Location? Location { get; set; }

    public virtual ICollection<RecordingSessionAlbum> RecordingSessionAlbums { get; set; } = new List<RecordingSessionAlbum>();
    public virtual ICollection<RecordingSessionTrack> RecordingSessionTracks { get; set; } = new List<RecordingSessionTrack>();
}
