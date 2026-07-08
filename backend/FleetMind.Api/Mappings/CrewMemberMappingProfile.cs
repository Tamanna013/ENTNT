using AutoMapper;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class CrewMemberMappingProfile : Profile
    {
        public CrewMemberMappingProfile()
        {
            CreateMap<CrewMember, CrewMemberDto>()
                .ForMember(dest => dest.ShipName, opt => opt.MapFrom(src => src.Ship != null ? src.Ship.Name : null));
            CreateMap<CreateCrewMemberDto, CrewMember>();
        }
    }
}
