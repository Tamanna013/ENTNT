using AutoMapper;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Incidents;

namespace FleetMind.Api.Mappings
{
    public class IncidentMappingProfile : Profile
    {
        public IncidentMappingProfile()
        {
            CreateMap<Incident, IncidentDto>()
                .ForMember(d => d.ShipName, opt => opt.MapFrom(s => s.Ship != null ? s.Ship.Name : string.Empty))
                .ForMember(d => d.VoyageNumber, opt => opt.MapFrom(s => s.Voyage != null ? s.Voyage.VoyageNumber : null))
                .ForMember(d => d.ReportedByUserName, opt => opt.MapFrom(s => s.ReportedByUser != null ? $"{s.ReportedByUser.FirstName} {s.ReportedByUser.LastName}".Trim() : string.Empty));
            
            CreateMap<CreateIncidentDto, Incident>();
            CreateMap<UpdateIncidentDto, Incident>();
        }
    }
}
