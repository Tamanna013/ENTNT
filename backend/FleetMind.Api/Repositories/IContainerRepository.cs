using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IContainerRepository : IGenericRepository<Container>
    {
        Task<(List<Container> items, int totalCount)> GetPagedAsync(ContainerQueryDto query);
        Task<bool> ExistsByContainerNumberAsync(string containerNumber, Guid? excludeId = null);
        Task<List<ContainerTrackingEvent>> GetTrackingEventsAsync(Guid containerId);
    }
}
