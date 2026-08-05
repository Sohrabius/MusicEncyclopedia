using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicEncyclopedia.Data.Entities;

[Table("Language")]
public class Language
{
    [Key]
    public int LanguageId { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    // Navigation
    public virtual ICollection<AlbumLanguage> AlbumLanguages { get; set; } = new List<AlbumLanguage>();
    public virtual ICollection<Alias> Aliases { get; set; } = new List<Alias>();
    public virtual ICollection<Localization> Localizations { get; set; } = new List<Localization>();
    public virtual ICollection<AttributeValue> AttributeValues { get; set; } = new List<AttributeValue>();
}
