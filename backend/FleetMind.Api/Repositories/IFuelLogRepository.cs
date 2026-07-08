using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IFuelLogRepository : IGenericRepository<FuelLog>
    {
        Task<(List<FuelLog> items, int totalCount)> GetPagedAsync(FuelLogQueryDto query);
    }
}
