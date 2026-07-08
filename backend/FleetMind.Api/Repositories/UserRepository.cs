using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Common;
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

    public async Task<(List<User> items, int totalCount)> GetPagedAsync(PaginationQueryDto query)
    {
        var dbQuery = _context.Users
            .Where(u => !u.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var searchTerm = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(u => 
                (u.FirstName + " " + u.LastName).ToLower().Contains(searchTerm) || 
                u.Email.ToLower().Contains(searchTerm));
        }

        var sortBy = query.SortBy?.ToLower();
        dbQuery = sortBy switch
        {
            "email" => query.SortDescending ? dbQuery.OrderByDescending(u => u.Email) : dbQuery.OrderBy(u => u.Email),
            "createdat" => query.SortDescending ? dbQuery.OrderByDescending(u => u.CreatedAt) : dbQuery.OrderBy(u => u.CreatedAt),
            _ => query.SortDescending ? dbQuery.OrderByDescending(u => u.CreatedAt) : dbQuery.OrderByDescending(u => u.CreatedAt), // default
        };

        var totalCount = await dbQuery.CountAsync();

        var items = await dbQuery
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
