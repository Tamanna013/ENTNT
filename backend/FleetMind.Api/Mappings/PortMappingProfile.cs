using AutoMapper;
using FleetMind.Api.DTOs.Ports;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings
{
    public class PortMappingProfile : Profile
    {
        public PortMappingProfile()
        {
            CreateMap<Port, PortDto>();
            CreateMap<CreatePortDto, Port>();
            CreateMap<UpdatePortDto, Port>();
        }
    }
}
