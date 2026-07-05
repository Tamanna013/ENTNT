using AutoMapper;
using FleetMind.Api.DTOs.Users;
using FleetMind.Api.Models;

namespace FleetMind.Api.Mappings;

/// <summary>
/// AutoMapper profile for User entity ↔ DTO mappings.
/// Only maps User → UserDto (output). CreateUserDto/UpdateUserDto → User
/// mappings are intentionally NOT defined here — those require password
/// hashing and role resolution logic that belongs in the service layer.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(
                dest => dest.Roles,
                opt => opt.MapFrom(src =>
                    src.UserRoles.Select(ur => ur.Role.Name).ToList()));
    }
}
