using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("ChartEntry")]
public class ChartEntry
{
    [Key]
    public int ChartEntryId { get; set; }

    public int ChartId { get; set; }

    public int EntityTypeId { get; set; }

    public int EntityId { get; set; }

    public DateOnly Date { get; set; }

    public int Position { get; set; }

    public int? PreviousPosition { get; set; }

    public int? WeeksOnChart { get; set; }

    // Navigation
    [ForeignKey(nameof(ChartId))]
    public virtual Chart Chart { get; set; } = null!;
}
