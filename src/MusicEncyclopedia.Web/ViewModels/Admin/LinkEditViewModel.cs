using System.ComponentModel.DataAnnotations;
using MusicEncyclopedia.Data.Entities;

namespace MusicEncyclopedia.Web.ViewModels.Admin;

/// <summary>
/// View model for the entity link create/edit form.
/// Maps to/from the <see cref="EntityLink"/> entity.
/// </summary>
public sealed class LinkEditViewModel
{
    public int EntityLinkId { get; set; }

    [Required(ErrorMessage = "Entity type is required.")]
    [Display(Name = "Entity Type")]
    public int EntityTypeId { get; set; }

    [Required(ErrorMessage = "Entity ID is required.")]
    [Display(Name = "Entity ID")]
    public int EntityId { get; set; }

    [Display(Name = "Link Type")]
    public int? LinkTypeId { get; set; }

    [Required(ErrorMessage = "URL is required.")]
    [StringLength(2000, ErrorMessage = "URL must not exceed 2000 characters.")]
    [Url(ErrorMessage = "Please enter a valid URL.")]
    [Display(Name = "URL")]
    public string Url { get; set; } = "";

    [StringLength(500, ErrorMessage = "Title must not exceed 500 characters.")]
    [Display(Name = "Title")]
    public string? Title { get; set; }

    // Dropdown data
    public IReadOnlyList<EntityType> EntityTypes { get; set; } = [];
    public IReadOnlyList<LinkType> LinkTypes { get; set; } = [];
}
