using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Containers;

namespace FleetMind.Api.Services
{
    public interface IContainerService
    {
        Task<PagedResultDto<ContainerDto>> GetContainersAsync(ContainerQueryDto query);
        Task<ContainerDto> GetContainerByIdAsync(Guid id);
        Task<ContainerDto> CreateContainerAsync(CreateContainerDto dto);
        Task<ContainerDto> UpdateContainerAsync(Guid id, UpdateContainerDto dto);
        Task DeleteContainerAsync(Guid id);
        Task<ContainerDto> LinkCargoAsync(Guid containerId, Guid cargoId);
        Task<ContainerDto> UnlinkCargoAsync(Guid containerId, Guid cargoId);
        Task<ContainerTrackingEventDto> RecordTrackingEventAsync(Guid containerId, RecordTrackingEventDto dto, Guid recordedByUserId);
        Task<List<ContainerTrackingEventDto>> GetTrackingEventsAsync(Guid containerId);
    }
}
