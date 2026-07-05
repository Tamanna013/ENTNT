namespace FleetMind.Api.Models.Common;

/// <summary>
/// Abstract base class providing shared audit and soft-delete fields
/// for all domain entities in the FleetMind system.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the entity was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last updated (UTC). Null if never updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Soft-delete flag. When true, the entity is logically deleted but retained in the database.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
}
