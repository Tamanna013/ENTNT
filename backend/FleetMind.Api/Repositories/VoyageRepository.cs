using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class VoyageRepository : GenericRepository<Voyage>, IVoyageRepository
    {
        public VoyageRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<Voyage?> GetByIdWithShipAsync(Guid id)
        {
            return await _dbSet
                .Include(v => v.Ship)
                .FirstOrDefaultAsync(v => v.Id == id && !v.IsDeleted);
        }

        public async Task<List<Guid>> GetOverdueVoyageIdsAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(v => !v.IsDeleted && 
                           (v.Status == "Scheduled" || v.Status == "InTransit") &&
                           v.EstimatedArrivalDate < now)
                .Select(v => v.Id)
                .ToListAsync();
        }

        public async Task<(List<Voyage> items, int totalCount)> GetPagedAsync(VoyageQueryDto query)
        {
            var queryable = _dbSet.AsNoTracking()
                .Include(v => v.Ship)
                .Include(v => v.OriginPort)
                .Include(v => v.DestinationPort)
                .Where(v => !v.IsDeleted);

            // 1. Search (Substring matching)
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                queryable = queryable.Where(v => 
                    v.VoyageNumber.ToLower().Contains(term) || 
                    (v.OriginPort != null && v.OriginPort.Name.ToLower().Contains(term)) || 
                    (v.DestinationPort != null && v.DestinationPort.Name.ToLower().Contains(term)));
            }

            // 2. Filters
            if (query.ShipId.HasValue)
            {
                queryable = queryable.Where(v => v.ShipId == query.ShipId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                queryable = queryable.Where(v => v.Status == query.Status);
            }

            if (query.DepartureFrom.HasValue)
            {
                queryable = queryable.Where(v => v.DepartureDate >= query.DepartureFrom.Value);
            }

            if (query.DepartureTo.HasValue)
            {
                queryable = queryable.Where(v => v.DepartureDate <= query.DepartureTo.Value);
            }

            // 3. Count before paging
            var totalCount = await queryable.CountAsync();

            // 4. Sort
            var isDesc = query.SortDescending;
            queryable = query.SortBy?.ToLower() switch
            {
                "voyagenumber" => isDesc ? queryable.OrderByDescending(v => v.VoyageNumber) : queryable.OrderBy(v => v.VoyageNumber),
                "createdat" => isDesc ? queryable.OrderByDescending(v => v.CreatedAt) : queryable.OrderBy(v => v.CreatedAt),
                _ => isDesc ? queryable.OrderByDescending(v => v.DepartureDate) : queryable.OrderBy(v => v.DepartureDate) // Default sort
            };

            // Eager load Ship for flattening
            queryable = queryable.Include(v => v.Ship);

            // 5. Page
            var skip = (query.PageNumber - 1) * query.PageSize;
            var items = await queryable.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByVoyageNumberAsync(string voyageNumber, Guid? excludeId = null)
        {
            return await _dbSet.AnyAsync(v => !v.IsDeleted && v.VoyageNumber == voyageNumber && v.Id != excludeId);
        }
    }
}
