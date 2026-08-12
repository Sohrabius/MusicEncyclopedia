using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AuditLog")]
public class AuditLog
{
    [Key]
    public int AuditLogId { get; set; }

    /// <summary>UTC timestamp of the audited action.</summary>
    public DateTime Timestamp { get; set; }

    [Required]
    [StringLength(255)]
    public string UserName { get; set; } = null!;

    /// <summary>Controller action, e.g. "Albums.Edit".</summary>
    [Required]
    [StringLength(255)]
    public string Action { get; set; } = null!;

    /// <summary>Entity type code, e.g. "Album".</summary>
    [StringLength(100)]
    public string? EntityType { get; set; }

    public int? EntityId { get; set; }

    public bool IsSuccess { get; set; }

    [StringLength(4000)]
    public string? Details { get; set; }

    [StringLength(100)]
    public string? IpAddress { get; set; }
}
