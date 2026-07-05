using System.Linq.Expressions;
using FleetMind.Api.Data;
using FleetMind.Api.Models.Common;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories;

/// <summary>
/// Generic repository implementation using EF Core.
/// All read queries use AsNoTracking() for performance and filter out soft-deleted records.
/// </summary>
public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly FleetMindDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(FleetMindDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);
    }

    public async Task<IReadOnlyList<T>> GetAllAsync()
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .Where(predicate)
            .ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Update(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }

    public void Remove(T entity)
    {
        entity.IsDeleted = true;
        entity.UpdatedAt = DateTime.UtcNow;
        _dbSet.Update(entity);
    }
}
