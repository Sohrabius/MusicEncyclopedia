using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("PerformanceEventAlbum")]
public class PerformanceEventAlbum
{
    [Key]
    public int PerformanceEventAlbumId { get; set; }

    public int PerformanceEventId { get; set; }

    public int AlbumId { get; set; }

    // Navigation
    [ForeignKey(nameof(PerformanceEventId))]
    public virtual PerformanceEvent PerformanceEvent { get; set; } = null!;

    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;
}
