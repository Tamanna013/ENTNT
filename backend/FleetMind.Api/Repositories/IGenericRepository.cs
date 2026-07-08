using System.Linq.Expressions;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Repositories;

/// <summary>
/// Generic repository interface for CRUD operations on any BaseEntity-derived type.
/// All read operations automatically filter out soft-deleted records (IsDeleted == true).
/// </summary>
public interface IGenericRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its unique identifier. Returns null if not found or soft-deleted.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    /// Gets all non-deleted entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync();

    /// <summary>
    /// Finds non-deleted entities matching the given predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    /// Counts non-deleted entities optionally matching the given predicate.
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    /// Adds a new entity to the context (not persisted until SaveChangesAsync is called).
    /// </summary>
    Task AddAsync(T entity);

    /// <summary>
    /// Marks an existing entity as modified.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Performs a soft delete by setting IsDeleted = true and UpdatedAt = UtcNow.
    /// The row is NOT removed from the database.
    /// </summary>
    void Remove(T entity);
}
