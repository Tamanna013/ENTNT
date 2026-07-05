using FleetMind.Api.Data;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories;

/// <summary>
/// User-specific repository implementation with custom query methods
/// for authentication and user management scenarios.
/// </summary>
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(FleetMindDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _dbSet
            .AsNoTracking()
            .AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() && !u.IsDeleted);
    }
}
