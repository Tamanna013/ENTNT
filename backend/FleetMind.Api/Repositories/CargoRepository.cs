using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Cargo;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class CargoRepository : GenericRepository<Models.Cargo>, ICargoRepository
    {
        public CargoRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<Models.Cargo> items, int totalCount)> GetPagedAsync(CargoQueryDto query)
        {
            var q = _dbSet.Include(c => c.Voyage).Where(c => !c.IsDeleted).AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                q = q.Where(c => c.Description.ToLower().Contains(searchTerm) || 
                                 c.ConsigneeName.ToLower().Contains(searchTerm));
            }

            if (query.VoyageId.HasValue)
            {
                q = q.Where(c => c.VoyageId == query.VoyageId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(c => c.Status == query.Status);
            }
            
            if (!string.IsNullOrWhiteSpace(query.Type))
            {
                q = q.Where(c => c.Type == query.Type);
            }

            var totalCount = await q.CountAsync();

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var sortBy = query.SortBy.ToLower();
                if (sortBy == "createdat")
                {
                    q = query.SortDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt);
                }
                else if (sortBy == "weightkg")
                {
                    q = query.SortDescending ? q.OrderByDescending(c => c.WeightKg) : q.OrderBy(c => c.WeightKg);
                }
                else
                {
                    q = query.SortDescending ? q.OrderByDescending(c => c.CreatedAt) : q.OrderBy(c => c.CreatedAt);
                }
            }
            else
            {
                q = q.OrderByDescending(c => c.CreatedAt);
            }

            var skip = (query.PageNumber - 1) * query.PageSize;
            var items = await q.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<decimal> GetTotalWeightForVoyageAsync(Guid voyageId)
        {
            return await _dbSet
                .Where(c => c.VoyageId == voyageId && !c.IsDeleted)
                .SumAsync(c => c.WeightKg);
        }
    }
}
