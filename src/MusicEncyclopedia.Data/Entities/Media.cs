using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Media")]
public class Media
{
    [Key]
    public int MediaId { get; set; }

    [Required]
    [StringLength(500)]
    public string FileName { get; set; } = null!;

    [Required]
    [StringLength(1000)]
    public string FilePath { get; set; } = null!;

    public int? MediaTypeId { get; set; }

    [StringLength(2000)]
    public string? Url { get; set; }

    [StringLength(2000)]
    public string? ThumbnailUrl150 { get; set; }

    [StringLength(2000)]
    public string? ThumbnailUrl300 { get; set; }

    [StringLength(2000)]
    public string? ThumbnailUrl600 { get; set; }

    [StringLength(2000)]
    public string? ThumbnailUrl1200 { get; set; }

    public int? Width { get; set; }

    public int? Height { get; set; }

    public long FileSize { get; set; }

    [Required]
    [StringLength(50)]
    public string MimeType { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    [Required]
    [StringLength(255)]
    public string CreatedBy { get; set; } = null!;

    public bool IsDeleted { get; set; }

    // Navigation
    [ForeignKey(nameof(MediaTypeId))]
    public virtual MediaType? MediaType { get; set; }

    public virtual ICollection<MediaAssignment> MediaAssignments { get; set; } = new List<MediaAssignment>();
    public virtual ICollection<Album> AlbumsAsCover { get; set; } = new List<Album>();
    public virtual ICollection<Person> PeopleAsImage { get; set; } = new List<Person>();
}
