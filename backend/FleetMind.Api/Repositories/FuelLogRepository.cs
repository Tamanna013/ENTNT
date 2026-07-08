using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public class FuelLogRepository : GenericRepository<FuelLog>, IFuelLogRepository
    {
        public FuelLogRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<FuelLog> items, int totalCount)> GetPagedAsync(FuelLogQueryDto query)
        {
            var queryable = _dbSet.AsNoTracking()
                                  .Include(f => f.Ship)
                                  .Include(f => f.Voyage)
                                  .Where(f => !f.IsDeleted);

            if (query.ShipId.HasValue)
                queryable = queryable.Where(f => f.ShipId == query.ShipId.Value);

            if (query.VoyageId.HasValue)
                queryable = queryable.Where(f => f.VoyageId == query.VoyageId.Value);

            if (!string.IsNullOrEmpty(query.FuelType))
                queryable = queryable.Where(f => f.FuelType == query.FuelType);

            int totalCount = await queryable.CountAsync();

            queryable = query.SortBy?.ToLower() switch
            {
                "createdat" => query.SortDescending ? queryable.OrderByDescending(f => f.CreatedAt) : queryable.OrderBy(f => f.CreatedAt),
                _ => query.SortDescending ? queryable.OrderByDescending(f => f.RecordedDate) : queryable.OrderBy(f => f.RecordedDate) // Default RecordedDate
            };

            var items = await queryable.Skip((query.PageNumber - 1) * query.PageSize)
                                       .Take(query.PageSize)
                                       .ToListAsync();

            return (items, totalCount);
        }
    }
}
