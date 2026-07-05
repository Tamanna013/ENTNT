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

    public UnitOfWork(FleetMindDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users =>
        _users ??= new UserRepository(_context);

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
