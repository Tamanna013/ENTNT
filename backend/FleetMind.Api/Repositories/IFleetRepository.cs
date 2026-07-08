using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IFleetRepository : IGenericRepository<Fleet>
    {
        Task<(List<Fleet> items, int totalCount)> GetPagedAsync(FleetQueryDto query);
        Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null);
    }
}
