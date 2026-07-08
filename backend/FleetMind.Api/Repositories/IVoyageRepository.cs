using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IVoyageRepository : IGenericRepository<Voyage>
    {
        Task<(List<Voyage> items, int totalCount)> GetPagedAsync(VoyageQueryDto query);
        Task<bool> ExistsByVoyageNumberAsync(string voyageNumber, Guid? excludeId = null);
        Task<Voyage?> GetByIdWithShipAsync(Guid id);
        Task<List<Guid>> GetOverdueVoyageIdsAsync();
    }
}
