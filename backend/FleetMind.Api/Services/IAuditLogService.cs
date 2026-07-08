using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Audit;

namespace FleetMind.Api.Services;

public interface IAuditLogService
{
    Task<PagedResultDto<AuditLogDto>> GetAuditLogsAsync(AuditLogQueryDto query);
}
