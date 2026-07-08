using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class FleetRepository : GenericRepository<Fleet>, IFleetRepository
    {
        public FleetRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<Fleet> items, int totalCount)> GetPagedAsync(FleetQueryDto query)
        {
            var queryable = _dbSet.AsNoTracking().Include(f => f.HomePort).Where(f => !f.IsDeleted);

            // 1. Filter
            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                queryable = queryable.Where(f => f.Status == query.Status);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                queryable = queryable.Where(f => 
                    f.Name.ToLower().Contains(search) || 
                    (f.Description != null && f.Description.ToLower().Contains(search)));
            }

            // 2. Count
            var totalCount = await queryable.CountAsync();

            // 3. Sort
            var sortBy = query.SortBy?.ToLower() ?? "createdat";
            var sortDescending = query.SortDescending;

            queryable = sortBy switch
            {
                "name" => sortDescending ? queryable.OrderByDescending(f => f.Name) : queryable.OrderBy(f => f.Name),
                _ => sortDescending ? queryable.OrderByDescending(f => f.CreatedAt) : queryable.OrderBy(f => f.CreatedAt)
            };

            // 4. Page
            var skip = (query.PageNumber - 1) * query.PageSize;
            var items = await queryable
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null)
        {
            var query = _dbSet.Where(f => f.Name.ToLower() == name.ToLower() && !f.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(f => f.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }
    }
}
