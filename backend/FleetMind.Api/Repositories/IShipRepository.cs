using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IShipRepository : IGenericRepository<Ship>
    {
        Task<(List<Ship> items, int totalCount)> GetPagedAsync(ShipQueryDto query);
        Task<bool> ExistsByImoAsync(string imo, Guid? excludeId = null);
        Task<int> CountByFleetIdAsync(Guid fleetId);
        Task<Dictionary<Guid, int>> GetShipCountsByFleetIdsAsync(IEnumerable<Guid> fleetIds);
        Task<Ship?> GetByIdWithFleetAsync(Guid id);
    }
}
