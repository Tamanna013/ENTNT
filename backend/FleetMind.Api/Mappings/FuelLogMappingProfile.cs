using AutoMapper;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class FuelLogMappingProfile : Profile
    {
        public FuelLogMappingProfile()
        {
            CreateMap<FuelLog, FuelLogDto>()
                .ForMember(dest => dest.ShipName, opt => opt.MapFrom(src => src.Ship != null ? src.Ship.Name : string.Empty))
                .ForMember(dest => dest.VoyageNumber, opt => opt.MapFrom(src => src.Voyage != null ? src.Voyage.VoyageNumber : null))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.QuantityLiters * src.CostPerLiter));

            CreateMap<CreateFuelLogDto, FuelLog>();
            CreateMap<UpdateFuelLogDto, FuelLog>();
        }
    }
}
