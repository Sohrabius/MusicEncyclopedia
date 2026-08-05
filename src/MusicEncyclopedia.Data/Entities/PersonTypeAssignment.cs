using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("PersonTypeAssignment")]
public class PersonTypeAssignment
{
    [Key]
    public int PersonTypeAssignmentId { get; set; }

    public int PersonId { get; set; }

    public int PersonTypeId { get; set; }

    // Navigation
    [ForeignKey(nameof(PersonId))]
    public virtual Person Person { get; set; } = null!;

    [ForeignKey(nameof(PersonTypeId))]
    public virtual PersonType PersonType { get; set; } = null!;
}
