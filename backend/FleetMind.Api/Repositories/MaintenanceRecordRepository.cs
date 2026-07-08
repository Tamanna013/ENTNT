using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FleetMind.Api.Data;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Repositories;

public class MaintenanceRecordRepository : GenericRepository<MaintenanceRecord>, IMaintenanceRecordRepository
{
    public MaintenanceRecordRepository(FleetMindDbContext context) : base(context)
    {
    }

    public async Task<(List<MaintenanceRecord> items, int totalCount)> GetPagedAsync(MaintenanceRecordQueryDto query)
    {
        var dbQuery = _dbSet.AsNoTracking().Where(m => !m.IsDeleted);

        if (query.ShipId.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.ShipId == query.ShipId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            dbQuery = dbQuery.Where(m => m.Status == query.Status);
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            dbQuery = dbQuery.Where(m => m.Type == query.Type);
        }

        if (query.DueBefore.HasValue)
        {
            dbQuery = dbQuery.Where(m => m.ScheduledDate <= query.DueBefore.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.ToLower();
            dbQuery = dbQuery.Where(m => m.Description.ToLower().Contains(search));
        }

        // Sorting
        if (string.Equals(query.SortBy, "createdAt", StringComparison.OrdinalIgnoreCase))
        {
            dbQuery = query.SortDescending
                ? dbQuery.OrderByDescending(m => m.CreatedAt)
                : dbQuery.OrderBy(m => m.CreatedAt);
        }
        else
        {
            // Default sort by ScheduledDate
            dbQuery = query.SortDescending
                ? dbQuery.OrderByDescending(m => m.ScheduledDate)
                : dbQuery.OrderBy(m => m.ScheduledDate);
        }

        var totalCount = await dbQuery.CountAsync();

        var items = await dbQuery
            .Include(m => m.Ship)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Guid>> GetOverdueRecordIdsAsync()
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .AsNoTracking()
            .Where(m => !m.IsDeleted && m.Status == MaintenanceStatus.Scheduled && m.ScheduledDate < now)
            .Select(m => m.Id)
            .ToListAsync();
    }
}
