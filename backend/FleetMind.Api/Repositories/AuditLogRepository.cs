using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Data;
using FleetMind.Api.DTOs.Audit;
using FleetMind.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Repositories;

public class AuditLogRepository : GenericRepository<AuditLog>, IAuditLogRepository
{
    public AuditLogRepository(FleetMindDbContext context) : base(context)
    {
    }

    public async Task<(List<AuditLog> items, int totalCount)> GetPagedAsync(AuditLogQueryDto query)
    {
        var queryable = _dbSet.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.EntityName))
        {
            queryable = queryable.Where(x => x.EntityName == query.EntityName);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            queryable = queryable.Where(x => x.Action == query.Action);
        }

        if (query.UserId.HasValue)
        {
            queryable = queryable.Where(x => x.UserId == query.UserId.Value);
        }

        if (query.DateFrom.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp >= query.DateFrom.Value);
        }

        if (query.DateTo.HasValue)
        {
            queryable = queryable.Where(x => x.Timestamp <= query.DateTo.Value);
        }

        var totalCount = await queryable.CountAsync();

        // Always sort descending chronologically
        queryable = queryable.OrderByDescending(x => x.Timestamp);

        var items = await queryable
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
