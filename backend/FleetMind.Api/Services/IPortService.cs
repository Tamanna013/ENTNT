using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ports;

namespace FleetMind.Api.Services
{
    public interface IPortService
    {
        Task<PagedResultDto<PortDto>> GetPortsAsync(PortQueryDto query);
        Task<PortDto> GetPortByIdAsync(Guid id);
        Task<PortDto> CreatePortAsync(CreatePortDto dto);
        Task<PortDto> UpdatePortAsync(Guid id, UpdatePortDto dto);
        Task DeactivatePortAsync(Guid id);
    }
}
