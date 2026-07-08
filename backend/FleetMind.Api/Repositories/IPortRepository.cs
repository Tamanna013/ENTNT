using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Ports;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IPortRepository : IGenericRepository<Port>
    {
        Task<(List<Port> items, int totalCount)> GetPagedAsync(PortQueryDto query);
        Task<bool> ExistsByUnLocodeAsync(string unLocode, Guid? excludeId = null);
    }
}
