using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories
{
    public class ContainerRepository : GenericRepository<Container>, IContainerRepository
    {
        public ContainerRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<(List<Container> items, int totalCount)> GetPagedAsync(ContainerQueryDto query)
        {
            var q = _dbSet
                .Include(c => c.CurrentVoyage)
                .Include(c => c.ContainerCargoItems)
                .Where(c => !c.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var searchTerm = query.SearchTerm.ToLower();
                q = q.Where(c => c.ContainerNumber.ToLower().Contains(searchTerm));
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                q = q.Where(c => c.Status == query.Status);
            }

            if (!string.IsNullOrWhiteSpace(query.Type))
            {
                q = q.Where(c => c.Type == query.Type);
            }

            if (query.VoyageId.HasValue)
            {
                q = q.Where(c => c.CurrentVoyageId == query.VoyageId.Value);
            }

            var totalCount = await q.CountAsync();

            if (!string.IsNullOrWhiteSpace(query.SortBy))
            {
                var sortBy = query.SortBy.ToLower();
                if (sortBy == "containernumber")
                {
                    q = query.SortDescending ? q.OrderByDescending(c => c.ContainerNumber) : q.OrderBy(c => c.ContainerNumber);
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

        public async Task<bool> ExistsByContainerNumberAsync(string containerNumber, Guid? excludeId = null)
        {
            var query = _dbSet.Where(c => c.ContainerNumber == containerNumber && !c.IsDeleted);
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.Id != excludeId.Value);
            }
            return await query.AnyAsync();
        }

        public async Task<List<ContainerTrackingEvent>> GetTrackingEventsAsync(Guid containerId)
        {
            return await _context.ContainerTrackingEvents
                .Where(e => e.ContainerId == containerId && !e.IsDeleted)
                .OrderByDescending(e => e.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
