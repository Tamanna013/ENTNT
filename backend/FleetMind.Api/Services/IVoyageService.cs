using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services
{
    public interface IVoyageService
    {
        Task<PagedResultDto<VoyageDto>> GetVoyagesAsync(VoyageQueryDto query);
        Task<VoyageDto> GetVoyageByIdAsync(Guid id);
        Task<VoyageDto> CreateVoyageAsync(CreateVoyageDto dto);
        Task<VoyageDto> UpdateVoyageAsync(Guid id, UpdateVoyageDto dto);
        Task DeleteVoyageAsync(Guid id);
        Task<VoyageDto> UpdateStatusAsync(Guid id, UpdateVoyageStatusDto dto);
        Task<PagedResultDto<VoyageDto>> GetVoyagesForShipAsync(Guid shipId, VoyageQueryDto query);
        Task<List<Guid>> GetOverdueVoyageIdsAsync();
        Task<AiSummaryResultDto> GetAiSummaryAsync(Guid voyageId);
    }
}
