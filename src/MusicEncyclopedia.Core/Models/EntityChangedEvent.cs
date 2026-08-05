namespace MusicEncyclopedia.Core.Models;

/// <summary>
/// Event raised when an entity is created, updated, deleted, or restored.
/// Used to trigger cache invalidation and other side effects.
/// </summary>
/// <param name="EntityTypeCode">The entity type code (e.g., "Album", "Track").</param>
/// <param name="EntityId">The ID of the entity that changed.</param>
/// <param name="ChangeType">The type of change (Created, Updated, Deleted, Restored).</param>
public sealed record EntityChangedEvent(
    string EntityTypeCode,
    int EntityId,
    string ChangeType);
