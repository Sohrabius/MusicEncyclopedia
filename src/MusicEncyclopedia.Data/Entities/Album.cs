using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Album")]
public class Album
{
    [Key]
    public int AlbumId { get; set; }

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

    public int AlbumCategoryId { get; set; }

    public DateOnly? ReleaseDate { get; set; }

    [StringLength(50)]
    public string? ReleaseDatePrecision { get; set; }

    public DateOnly? RecordingStartDate { get; set; }

    public DateOnly? RecordingEndDate { get; set; }

    [StringLength(50)]
    public string? RecordingDatePrecision { get; set; }

    public string? Description { get; set; }

    public int? CoverMediaId { get; set; }

    public int? DurationSeconds { get; set; }

    [StringLength(1000)]
    public string? CopyrightNotice { get; set; }

    [Required]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    public bool IsOfficial { get; set; }

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

    [ForeignKey(nameof(AlbumCategoryId))]
    public virtual AlbumCategory AlbumCategory { get; set; } = null!;

    [ForeignKey(nameof(CoverMediaId))]
    public virtual Media? CoverMedia { get; set; }

    public virtual ICollection<AlbumTrack> AlbumTracks { get; set; } = new List<AlbumTrack>();
    public virtual ICollection<AlbumGenre> AlbumGenres { get; set; } = new List<AlbumGenre>();
    public virtual ICollection<AlbumMood> AlbumMoods { get; set; } = new List<AlbumMood>();
    public virtual ICollection<AlbumLanguage> AlbumLanguages { get; set; } = new List<AlbumLanguage>();
    public virtual ICollection<AlbumCountry> AlbumCountries { get; set; } = new List<AlbumCountry>();
    public virtual ICollection<AlbumCompany> AlbumCompanies { get; set; } = new List<AlbumCompany>();
    public virtual ICollection<AlbumIdentifier> AlbumIdentifiers { get; set; } = new List<AlbumIdentifier>();
    public virtual ICollection<RecordingSessionAlbum> RecordingSessionAlbums { get; set; } = new List<RecordingSessionAlbum>();
    public virtual ICollection<PerformanceEventAlbum> PerformanceEventAlbums { get; set; } = new List<PerformanceEventAlbum>();
    public virtual ICollection<AlbumRelation> AlbumRelations { get; set; } = new List<AlbumRelation>();
    public virtual ICollection<AlbumRelation> RelatedAlbumRelations { get; set; } = new List<AlbumRelation>();
}
