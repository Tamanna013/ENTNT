using FleetMind.Api.Data;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Repositories;

/// <summary>
/// Unit of Work interface that coordinates repository access and transactional persistence.
/// All pending changes across repositories are committed atomically via SaveChangesAsync.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// User-specific repository with custom query methods.
    /// </summary>
    IUserRepository Users { get; }

    /// <summary>
    /// Factory method returning a generic repository for any BaseEntity-derived type
    /// that doesn't need custom repository methods.
    /// </summary>
    IGenericRepository<T> Repository<T>() where T : BaseEntity;

    /// <summary>
    /// Direct access to the underlying DbContext for operations on non-BaseEntity types
    /// (e.g., join entities like UserRole) that don't fit the generic repository pattern.
    /// </summary>
    FleetMindDbContext Context { get; }

    /// <summary>
    /// Commits all pending changes to the database in a single transaction.
    /// </summary>
    Task<int> SaveChangesAsync();
}
