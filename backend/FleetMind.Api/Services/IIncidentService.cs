using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services
{
    public interface IIncidentService
    {
        Task<PagedResultDto<IncidentDto>> GetIncidentsAsync(IncidentQueryDto query);
        Task<IncidentDto> GetIncidentByIdAsync(Guid id);
        Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto);
        Task<IncidentDto> UpdateIncidentAsync(Guid id, UpdateIncidentDto dto);
        Task DeleteIncidentAsync(Guid id);
        Task<IncidentDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusDto dto);
        Task<AiSummaryResultDto> GetAiReportAsync(Guid incidentId);
        Task<PagedResultDto<IncidentDto>> GetIncidentsForShipAsync(Guid shipId, IncidentQueryDto query);
    }
}
