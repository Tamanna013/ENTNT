using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Fleets;

namespace FleetMind.Api.Services
{
    public interface IFleetService
    {
        Task<PagedResultDto<FleetDto>> GetFleetsAsync(FleetQueryDto query);
        Task<FleetDto> GetFleetByIdAsync(Guid id);
        Task<FleetDto> CreateFleetAsync(CreateFleetDto dto);
        Task<FleetDto> UpdateFleetAsync(Guid id, UpdateFleetDto dto);
        Task<bool> DeactivateFleetAsync(Guid id);
    }
}
