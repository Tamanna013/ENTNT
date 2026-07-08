using AutoMapper;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class FleetMappingProfile : Profile
    {
        public FleetMappingProfile()
        {
            CreateMap<Fleet, FleetDto>()
                .ForMember(dest => dest.ShipCount, opt => opt.Ignore())
                .ForMember(dest => dest.HomePortName, opt => opt.MapFrom(src => src.HomePort != null ? src.HomePort.Name : string.Empty));

            CreateMap<CreateFleetDto, Fleet>();
            CreateMap<UpdateFleetDto, Fleet>();
        }
    }
}
