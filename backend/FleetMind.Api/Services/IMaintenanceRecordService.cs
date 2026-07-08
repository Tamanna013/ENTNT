using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Maintenance;

namespace FleetMind.Api.Services;

public interface IMaintenanceRecordService
{
    Task<PagedResultDto<MaintenanceRecordDto>> GetMaintenanceRecordsAsync(MaintenanceRecordQueryDto query);
    Task<MaintenanceRecordDto> GetMaintenanceRecordByIdAsync(Guid id);
    Task<MaintenanceRecordDto> CreateMaintenanceRecordAsync(CreateMaintenanceRecordDto dto);
    Task<MaintenanceRecordDto> UpdateMaintenanceRecordAsync(Guid id, UpdateMaintenanceRecordDto dto);
    Task DeleteMaintenanceRecordAsync(Guid id);
    
    Task<MaintenanceRecordDto> UpdateStatusAsync(Guid id, UpdateMaintenanceStatusDto dto);
    
    Task<PagedResultDto<MaintenanceRecordDto>> GetMaintenanceForShipAsync(Guid shipId, MaintenanceRecordQueryDto query);
    
    Task<List<Guid>> GetOverdueRecordIdsAsync();
    
    // Internal method to be called ONLY by the background service
    Task MarkOverdueAsync(Guid id);
}
