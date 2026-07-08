using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Audit;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories;

public interface IAuditLogRepository : IGenericRepository<AuditLog>
{
    Task<(List<AuditLog> items, int totalCount)> GetPagedAsync(AuditLogQueryDto query);
}
