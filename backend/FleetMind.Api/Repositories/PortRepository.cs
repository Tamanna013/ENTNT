using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Ports;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class PortRepository : GenericRepository<Port>, IPortRepository
    {
        public PortRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<Port> items, int totalCount)> GetPagedAsync(PortQueryDto query)
        {
            var queryable = _dbSet.AsNoTracking().Where(p => !p.IsDeleted);

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                queryable = queryable.Where(p => 
                    p.Name.ToLower().Contains(search) || 
                    p.City.ToLower().Contains(search) || 
                    p.UnLocode.ToLower().Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(query.Country))
            {
                queryable = queryable.Where(p => p.Country.ToLower() == query.Country.ToLower());
            }

            // Apply sorting
            if (query.SortBy?.ToLower() == "createdat")
            {
                queryable = query.SortDescending 
                    ? queryable.OrderByDescending(p => p.CreatedAt)
                    : queryable.OrderBy(p => p.CreatedAt);
            }
            else
            {
                // Default sort by Name
                queryable = query.SortDescending
                    ? queryable.OrderByDescending(p => p.Name)
                    : queryable.OrderBy(p => p.Name);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByUnLocodeAsync(string unLocode, Guid? excludeId = null)
        {
            return await _dbSet.AnyAsync(p => !p.IsDeleted && p.UnLocode == unLocode && p.Id != excludeId);
        }
    }
}
