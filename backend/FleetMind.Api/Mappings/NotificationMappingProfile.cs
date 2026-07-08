using AutoMapper;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Notifications;

namespace FleetMind.Api.Mappings;

public class NotificationMappingProfile : Profile
{
    public NotificationMappingProfile()
    {
        CreateMap<Notification, NotificationDto>();
    }
}
