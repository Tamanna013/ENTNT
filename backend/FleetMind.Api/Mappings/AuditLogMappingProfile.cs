using AutoMapper;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Audit;

namespace FleetMind.Api.Mappings;

public class AuditLogMappingProfile : Profile
{
    public AuditLogMappingProfile()
    {
        CreateMap<AuditLog, AuditLogDto>();
    }
}
