using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Cargo;

namespace FleetMind.Api.Repositories
{
    public interface ICargoRepository : IGenericRepository<Models.Cargo>
    {
        Task<(List<Models.Cargo> items, int totalCount)> GetPagedAsync(CargoQueryDto query);
        Task<decimal> GetTotalWeightForVoyageAsync(Guid voyageId);
    }
}
