using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Maintenance;

namespace FleetMind.Api.Repositories;

public interface IMaintenanceRecordRepository : IGenericRepository<MaintenanceRecord>
{
    Task<(List<MaintenanceRecord> items, int totalCount)> GetPagedAsync(MaintenanceRecordQueryDto query);
    Task<List<Guid>> GetOverdueRecordIdsAsync();
}
