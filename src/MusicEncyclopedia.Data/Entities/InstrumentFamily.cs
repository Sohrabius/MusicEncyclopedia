using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("InstrumentFamily")]
public class InstrumentFamily
{
    [Key]
    public int InstrumentFamilyId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<Instrument> Instruments { get; set; } = new List<Instrument>();
}
