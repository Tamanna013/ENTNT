using AutoMapper;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class ShipMappingProfile : Profile
    {
        public ShipMappingProfile()
        {
            CreateMap<Ship, ShipDto>()
                .ForMember(dest => dest.FleetName, opt => opt.MapFrom(src => src.Fleet != null ? src.Fleet.Name : string.Empty))
                .ForMember(dest => dest.PrimaryPhotoUrl, opt => opt.MapFrom(src => 
                    src.PrimaryPhotoAttachmentId.HasValue 
                        ? $"/api/v1/attachments/{src.PrimaryPhotoAttachmentId}/download" 
                        : null));

            CreateMap<CreateShipDto, Ship>();
            CreateMap<UpdateShipDto, Ship>();
        }
    }
}
