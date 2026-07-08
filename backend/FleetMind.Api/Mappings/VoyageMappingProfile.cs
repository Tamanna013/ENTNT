using AutoMapper;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class VoyageMappingProfile : Profile
    {
        public VoyageMappingProfile()
        {
            CreateMap<Voyage, VoyageDto>()
                .ForMember(dest => dest.ShipName, opt => opt.MapFrom(src => src.Ship != null ? src.Ship.Name : string.Empty))
                .ForMember(dest => dest.OriginPortName, opt => opt.MapFrom(src => src.OriginPort != null ? src.OriginPort.Name : string.Empty))
                .ForMember(dest => dest.DestinationPortName, opt => opt.MapFrom(src => src.DestinationPort != null ? src.DestinationPort.Name : string.Empty));
                
            CreateMap<CreateVoyageDto, Voyage>();
            CreateMap<UpdateVoyageDto, Voyage>();
        }
    }
}
