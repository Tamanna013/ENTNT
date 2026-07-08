using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public class CrewMemberRepository : GenericRepository<CrewMember>, ICrewMemberRepository
    {
        public CrewMemberRepository(FleetMindDbContext context) : base(context)
        {
        }

        public async Task<CrewMember?> GetByIdWithShipAsync(Guid id)
        {
            return await _dbSet.Include(c => c.Ship).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<(List<CrewMember> items, int totalCount)> GetPagedAsync(CrewMemberQueryDto query)
        {
            var queryable = _dbSet.AsNoTracking().Where(c => !c.IsDeleted);

            // Substring search on Name or LicenseNumber
            if (!string.IsNullOrWhiteSpace(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                queryable = queryable.Where(c => 
                    c.FirstName.ToLower().Contains(term) || 
                    c.LastName.ToLower().Contains(term) ||
                    c.LicenseNumber.ToLower().Contains(term));
            }

            // ShipId / Unassigned logic
            if (query.Unassigned == true)
            {
                queryable = queryable.Where(c => c.ShipId == null);
            }
            else if (query.ShipId.HasValue)
            {
                queryable = queryable.Where(c => c.ShipId == query.ShipId.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Status))
            {
                queryable = queryable.Where(c => c.Status == query.Status);
            }

            if (!string.IsNullOrWhiteSpace(query.Rank))
            {
                queryable = queryable.Where(c => c.Rank == query.Rank);
            }

            // Get total count before paging
            var totalCount = await queryable.CountAsync();

            // Sorting
            var isDesc = query.SortDescending;
            queryable = query.SortBy?.ToLower() switch
            {
                "lastname" => isDesc ? queryable.OrderByDescending(c => c.LastName) : queryable.OrderBy(c => c.LastName),
                "hiredate" => isDesc ? queryable.OrderByDescending(c => c.HireDate) : queryable.OrderBy(c => c.HireDate),
                _ => isDesc ? queryable.OrderByDescending(c => c.CreatedAt) : queryable.OrderBy(c => c.CreatedAt)
            };

            // Conditionally include Ship for the flattened ShipName
            queryable = queryable.Include(c => c.Ship);

            // Paging
            var skip = (query.PageNumber - 1) * query.PageSize;
            var items = await queryable.Skip(skip).Take(query.PageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludeId = null)
        {
            return await _dbSet.AnyAsync(c => !c.IsDeleted && c.LicenseNumber == licenseNumber && c.Id != excludeId);
        }
    }
}
