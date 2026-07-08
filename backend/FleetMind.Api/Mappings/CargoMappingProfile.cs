using AutoMapper;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class CargoMappingProfile : Profile
    {
        public CargoMappingProfile()
        {
            CreateMap<Cargo, CargoDto>()
                .ForMember(dest => dest.VoyageNumber, opt => opt.MapFrom(src => src.Voyage != null ? src.Voyage.VoyageNumber : string.Empty))
                .ForMember(dest => dest.Warnings, opt => opt.Ignore()); // Warnings are populated manually by the service, not mapped
                
            CreateMap<CreateCargoDto, Cargo>();
        }
    }
}
