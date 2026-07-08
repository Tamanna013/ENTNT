using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.Models;

namespace FleetMind.Api.Repositories
{
    public interface ICrewMemberRepository : IGenericRepository<CrewMember>
    {
        Task<CrewMember?> GetByIdWithShipAsync(Guid id);
        Task<(List<CrewMember> items, int totalCount)> GetPagedAsync(CrewMemberQueryDto query);
        Task<bool> ExistsByLicenseNumberAsync(string licenseNumber, Guid? excludeId = null);
    }
}
