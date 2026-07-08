using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Users;

namespace FleetMind.Api.Services
{
    public interface IUserManagementService
    {
        Task<PagedResultDto<UserDto>> GetUsersAsync(PaginationQueryDto query);
        Task<UserDto> GetUserByIdAsync(Guid id);
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<UserDto> UpdateUserAsync(Guid id, UpdateUserDto dto);
        Task DeactivateUserAsync(Guid id);

        Task<UserSettingsDto> GetMySettingsAsync();
        Task<UserSettingsDto> UpdateMySettingsAsync(UpdateUserSettingsDto dto);
        Task<UserDto> UpdateOwnProfileAsync(UpdateOwnProfileDto dto);
        Task<UserDto> AssignRolesAsync(Guid id, AssignRolesDto dto);
        Task<UserDto> GetCurrentUserAsync();
    }
}
