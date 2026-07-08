using System;
using System.Threading.Tasks;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Users;
using FleetMind.Api.Services;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : ControllerBase
    {
        private readonly IUserManagementService _userService;

        public UsersController(IUserManagementService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsers([FromQuery] PaginationQueryDto query)
        {
            var result = await _userService.GetUsersAsync(query);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);
            return CreatedAtAction(nameof(GetUserById), new { version = "1.0", id = user.Id }, user);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<UserDto>> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
        {
            var user = await _userService.UpdateUserAsync(id, dto);
            return Ok(user);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult> DeactivateUser(Guid id)
        {
            await _userService.DeactivateUserAsync(id);
            return NoContent();
        }

        [HttpPut("{id:guid}/roles")]
        public async Task<ActionResult<UserDto>> AssignRoles(Guid id, [FromBody] AssignRolesDto dto)
        {
            var user = await _userService.AssignRolesAsync(id, dto);
            return Ok(user);
        }

        [HttpGet("me")]
        [Authorize] // Overrides the AdminOnly policy to allow any authenticated user
        public async Task<ActionResult<UserDto>> GetMe()
        {
            var user = await _userService.GetCurrentUserAsync();
            return Ok(user);
        }

        [HttpGet("me/settings")]
        [Authorize] // Overrides the AdminOnly policy to allow any authenticated user
        public async Task<ActionResult<UserSettingsDto>> GetMySettings()
        {
            var settings = await _userService.GetMySettingsAsync();
            return Ok(settings);
        }

        [HttpPut("me/settings")]
        [Authorize] // Overrides the AdminOnly policy to allow any authenticated user
        public async Task<ActionResult<UserSettingsDto>> UpdateMySettings([FromBody] UpdateUserSettingsDto dto)
        {
            var settings = await _userService.UpdateMySettingsAsync(dto);
            return Ok(settings);
        }

        [HttpPut("me/profile")]
        [Authorize] // Overrides the AdminOnly policy to allow any authenticated user
        public async Task<ActionResult<UserDto>> UpdateMyProfile([FromBody] UpdateOwnProfileDto dto)
        {
            var user = await _userService.UpdateOwnProfileAsync(dto);
            return Ok(user);
        }
    }
}
