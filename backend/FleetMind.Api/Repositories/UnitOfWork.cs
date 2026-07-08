using System.Collections.Concurrent;
using FleetMind.Api.Data;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Repositories;

/// <summary>
/// Unit of Work implementation coordinating FleetMindDbContext and all repositories.
/// Repositories are lazily instantiated and cached for the lifetime of the UoW scope.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly FleetMindDbContext _context;
    private readonly ConcurrentDictionary<Type, object> _repositories = new();

    private IUserRepository? _users;
    private IRefreshTokenRepository? _refreshTokens;
    private IFleetRepository? _fleets;
    private IShipRepository? _ships;
    private ICrewMemberRepository? _crewMembers;
    private IVoyageRepository? _voyages;
    private ICargoRepository? _cargo;
    private IContainerRepository? _containers;
    private IPortRepository? _ports;
    private IReportingRepository? _reporting;
    private IAuditLogRepository? _auditLogs;
    private IMaintenanceRecordRepository? _maintenanceRecords;
    private IFuelLogRepository? _fuelLogs;
    private IIncidentRepository? _incidents;
    private IDocumentRepository? _documents;

    public UnitOfWork(FleetMindDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

    public IRefreshTokenRepository RefreshTokens =>
        _refreshTokens ??= new RefreshTokenRepository(_context);

    public IFleetRepository Fleets =>
        _fleets ??= new FleetRepository(_context);

    public IShipRepository Ships =>
        _ships ??= new ShipRepository(_context);

    public ICrewMemberRepository CrewMembers =>
        _crewMembers ??= new CrewMemberRepository(_context);

    public IVoyageRepository Voyages =>
        _voyages ??= new VoyageRepository(_context);

    public ICargoRepository Cargo =>
        _cargo ??= new CargoRepository(_context);

    public IContainerRepository Containers => _containers ??= new ContainerRepository(_context);
    public IReportingRepository Reporting => _reporting ??= new ReportingRepository(_context);
    public IAuditLogRepository AuditLogs => _auditLogs ??= new AuditLogRepository(_context);
    public IPortRepository Ports => _ports ??= new PortRepository(_context);
    public IMaintenanceRecordRepository MaintenanceRecords => _maintenanceRecords ??= new MaintenanceRecordRepository(_context);
    public IFuelLogRepository FuelLogs => _fuelLogs ??= new FuelLogRepository(_context);
    public IIncidentRepository Incidents => _incidents ??= new IncidentRepository(_context);
    public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);

    public IGenericRepository<T> Repository<T>() where T : BaseEntity
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(
            typeof(T),
            _ => new GenericRepository<T>(_context));
    }

    public FleetMindDbContext Context => _context;

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
