using System.Linq;
using AutoMapper;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class ContainerMappingProfile : Profile
    {
        public ContainerMappingProfile()
        {
            CreateMap<Container, ContainerDto>()
                .ForMember(dest => dest.VoyageNumber, opt => opt.MapFrom(src => src.CurrentVoyage != null ? src.CurrentVoyage.VoyageNumber : null))
                .ForMember(dest => dest.LinkedCargoIds, opt => opt.MapFrom(src => src.ContainerCargoItems != null ? src.ContainerCargoItems.Select(cci => cci.CargoId).ToList() : new System.Collections.Generic.List<System.Guid>()));

            CreateMap<CreateContainerDto, Container>();
            
            // ContainerTrackingEventDto will mostly be mapped explicitly or have its RecordedByUserName resolved separately
            CreateMap<ContainerTrackingEvent, ContainerTrackingEventDto>()
                .ForMember(dest => dest.RecordedByUserName, opt => opt.Ignore());
        }
    }
}
