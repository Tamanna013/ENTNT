using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Crew;

namespace FleetMind.Api.Services
{
    public interface ICrewMemberService
    {
        Task<PagedResultDto<CrewMemberDto>> GetCrewMembersAsync(CrewMemberQueryDto query);
        Task<CrewMemberDto> GetCrewMemberByIdAsync(Guid id);
        Task<CrewMemberDto> CreateCrewMemberAsync(CreateCrewMemberDto dto);
        Task<CrewMemberDto> UpdateCrewMemberAsync(Guid id, UpdateCrewMemberDto dto);
        Task DeactivateCrewMemberAsync(Guid id);
        Task<CrewMemberDto> AssignToShipAsync(Guid id, Guid shipId);
        Task<CrewMemberDto> UnassignFromShipAsync(Guid id);
        Task<PagedResultDto<CrewMemberDto>> GetCrewForShipAsync(Guid shipId, CrewMemberQueryDto query);
        
        Task<System.Collections.Generic.List<CrewCertificationDto>> GetCertificationsAsync(Guid crewMemberId);
        Task<CrewCertificationDto> UploadCertificationAsync(Guid crewMemberId, Microsoft.AspNetCore.Http.IFormFile file, string certificationName, DateOnly expiryDate, Guid uploadedByUserId);
        Task DeleteCertificationAsync(Guid crewMemberId, Guid certificationId);
    }
}
