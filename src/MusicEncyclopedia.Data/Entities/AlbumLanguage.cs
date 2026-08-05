using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("AlbumLanguage")]
public class AlbumLanguage
{
    [Key]
    public int AlbumLanguageId { get; set; }

    public int AlbumId { get; set; }

    public int? LanguageId { get; set; }

    // Navigation
    [ForeignKey(nameof(AlbumId))]
    public virtual Album Album { get; set; } = null!;

    [ForeignKey(nameof(LanguageId))]
    public virtual Language? Language { get; set; }
}
