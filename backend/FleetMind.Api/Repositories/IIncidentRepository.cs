using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface IIncidentRepository : IGenericRepository<Incident>
    {
        Task<(List<Incident> items, int totalCount)> GetPagedAsync(IncidentQueryDto query);
    }
}
