using AutoMapper;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Maintenance;

namespace FleetMind.Api.Mappings;

public class MaintenanceRecordMappingProfile : Profile
{
    public MaintenanceRecordMappingProfile()
    {
        CreateMap<MaintenanceRecord, MaintenanceRecordDto>()
            .ForMember(dest => dest.ShipName, opt => opt.MapFrom(src => src.Ship != null ? src.Ship.Name : string.Empty));
    }
}
