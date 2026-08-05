using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Chart")]
public class Chart
{
    [Key]
    public int ChartId { get; set; }

    public int EntityId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Publisher { get; set; }

    public int? CountryId { get; set; }

    [StringLength(50)]
    public string? Frequency { get; set; }

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

    [ForeignKey(nameof(CountryId))]
    public virtual Country? Country { get; set; }

    public virtual ICollection<ChartEntry> ChartEntries { get; set; } = new List<ChartEntry>();
}
