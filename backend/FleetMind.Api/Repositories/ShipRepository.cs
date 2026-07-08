using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class ShipRepository : GenericRepository<Ship>, IShipRepository
    {
        public ShipRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<Ship> items, int totalCount)> GetPagedAsync(ShipQueryDto query)
        {
            var queryable = _dbSet.Include(s => s.Fleet).AsNoTracking().Where(s => !s.IsDeleted);

            // 1. Filter
            if (query.FleetId.HasValue)
            {
                queryable = queryable.Where(s => s.FleetId == query.FleetId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                queryable = queryable.Where(s => s.Status == query.Status);
            }

            if (!string.IsNullOrWhiteSpace(query.Type))
            {
                queryable = queryable.Where(s => s.Type == query.Type);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                queryable = queryable.Where(s => 
                    s.Name.ToLower().Contains(search) || 
                    s.IMO.ToLower().Contains(search));
            }

            // 2. Count
            var totalCount = await queryable.CountAsync();

            // 3. Sort
            var sortBy = query.SortBy?.ToLower() ?? "createdat";
            var sortDescending = query.SortDescending;

            queryable = sortBy switch
            {
                "name" => sortDescending ? queryable.OrderByDescending(s => s.Name) : queryable.OrderBy(s => s.Name),
                "imo" => sortDescending ? queryable.OrderByDescending(s => s.IMO) : queryable.OrderBy(s => s.IMO),
                _ => sortDescending ? queryable.OrderByDescending(s => s.CreatedAt) : queryable.OrderBy(s => s.CreatedAt)
            };

            // 4. Page
            var skip = (query.PageNumber - 1) * query.PageSize;
            var items = await queryable
                .Skip(skip)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByImoAsync(string imo, Guid? excludeId = null)
        {
            var query = _dbSet.Where(s => s.IMO.ToLower() == imo.ToLower() && !s.IsDeleted);

            if (excludeId.HasValue)
            {
                query = query.Where(s => s.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<int> CountByFleetIdAsync(Guid fleetId)
        {
            return await _dbSet.CountAsync(s => s.FleetId == fleetId && !s.IsDeleted);
        }

        public async Task<Dictionary<Guid, int>> GetShipCountsByFleetIdsAsync(IEnumerable<Guid> fleetIds)
        {
            var fleetIdsList = fleetIds.ToList();
            if (!fleetIdsList.Any()) return new Dictionary<Guid, int>();

            var counts = await _dbSet
                .Where(s => fleetIdsList.Contains(s.FleetId) && !s.IsDeleted)
                .GroupBy(s => s.FleetId)
                .Select(g => new { FleetId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.FleetId, x => x.Count);

            return counts;
        }

        public async Task<Ship?> GetByIdWithFleetAsync(Guid id)
        {
            return await _dbSet.Include(s => s.Fleet).FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }
    }
}
