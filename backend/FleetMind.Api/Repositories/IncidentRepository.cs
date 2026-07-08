using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class IncidentRepository : GenericRepository<Incident>, IIncidentRepository
    {
        public IncidentRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<Incident> items, int totalCount)> GetPagedAsync(IncidentQueryDto query)
        {
            var queryable = _dbSet
                .Include(i => i.Ship)
                .Include(i => i.Voyage)
                .Include(i => i.ReportedByUser)
                .AsNoTracking()
                .Where(i => !i.IsDeleted);

            if (query.ShipId.HasValue)
            {
                queryable = queryable.Where(i => i.ShipId == query.ShipId.Value);
            }

            if (query.VoyageId.HasValue)
            {
                queryable = queryable.Where(i => i.VoyageId == query.VoyageId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                queryable = queryable.Where(i => i.Status == query.Status);
            }

            if (!string.IsNullOrWhiteSpace(query.Severity))
            {
                queryable = queryable.Where(i => i.Severity == query.Severity);
            }

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var search = query.SearchTerm.ToLower();
                queryable = queryable.Where(i => 
                    i.Title.ToLower().Contains(search) || 
                    i.Description.ToLower().Contains(search));
            }

            // Apply sorting
            if (query.SortBy?.ToLower() == "createdat")
            {
                queryable = query.SortDescending 
                    ? queryable.OrderByDescending(i => i.CreatedAt)
                    : queryable.OrderBy(i => i.CreatedAt);
            }
            else
            {
                // Default sort by OccurredAt descending
                queryable = query.SortDescending
                    ? queryable.OrderBy(i => i.OccurredAt)
                    : queryable.OrderByDescending(i => i.OccurredAt);
            }

            var totalCount = await queryable.CountAsync();

            var items = await queryable
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
