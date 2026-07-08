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
    /// Refresh token specific repository.
    /// </summary>
    IRefreshTokenRepository RefreshTokens { get; }

    /// <summary>
    /// Fleet specific repository.
    /// </summary>
    IFleetRepository Fleets { get; }

    /// <summary>
    /// Ship specific repository.
    /// </summary>
    IShipRepository Ships { get; }

    /// <summary>
    /// Crew member specific repository.
    /// </summary>
    ICrewMemberRepository CrewMembers { get; }

    /// <summary>
    /// Voyage specific repository.
    /// </summary>
    IVoyageRepository Voyages { get; }

    /// <summary>
    /// Cargo specific repository.
    /// </summary>
    ICargoRepository Cargo { get; }

    /// <summary>
    /// Container specific repository.
    /// </summary>
    IContainerRepository Containers { get; }

    /// <summary>
    /// Port specific repository.
    /// </summary>
    IPortRepository Ports { get; }

    /// <summary>
    /// Reporting specific repository.
    /// </summary>
    IReportingRepository Reporting { get; }
    
    /// <summary>
    /// Audit log specific repository.
    /// </summary>
    IAuditLogRepository AuditLogs { get; }

    /// <summary>
    /// Maintenance record specific repository.
    /// </summary>
    IMaintenanceRecordRepository MaintenanceRecords { get; }

    /// <summary>
    /// Fuel log specific repository.
    /// </summary>
    IFuelLogRepository FuelLogs { get; }

    /// <summary>
    /// Incident specific repository.
    /// </summary>
    IIncidentRepository Incidents { get; }

    /// <summary>
    /// Document specific repository.
    /// </summary>
    IDocumentRepository Documents { get; }

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
