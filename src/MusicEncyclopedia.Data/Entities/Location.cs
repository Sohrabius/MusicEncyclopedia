using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Location")]
public class Location
{
    [Key]
    public int LocationId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public int? LocationTypeId { get; set; }

    public int? ParentLocationId { get; set; }

    public int? CountryId { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(11,8)")]
    public decimal? Longitude { get; set; }

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

    [ForeignKey(nameof(LocationTypeId))]
    public virtual LocationType? LocationType { get; set; }

    [ForeignKey(nameof(ParentLocationId))]
    public virtual Location? ParentLocation { get; set; }

    public virtual ICollection<Location> ChildLocations { get; set; } = new List<Location>();

    [ForeignKey(nameof(CountryId))]
    public virtual Country? Country { get; set; }

    public virtual ICollection<Person> PeopleBornHere { get; set; } = new List<Person>();
    public virtual ICollection<Person> PeopleDiedHere { get; set; } = new List<Person>();
    public virtual ICollection<RecordingSession> RecordingSessions { get; set; } = new List<RecordingSession>();
    public virtual ICollection<PerformanceEvent> PerformanceEvents { get; set; } = new List<PerformanceEvent>();
}
