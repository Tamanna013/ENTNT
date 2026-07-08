using System;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/containers")]
    [ApiVersion("1.0")]
    [Authorize]
    public class ContainersController : ControllerBase
    {
        private readonly IContainerService _containerService;

        public ContainersController(IContainerService containerService)
        {
            _containerService = containerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetContainers([FromQuery] ContainerQueryDto query)
        {
            var result = await _containerService.GetContainersAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContainerById(Guid id)
        {
            var container = await _containerService.GetContainerByIdAsync(id);
            return Ok(container);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> CreateContainer([FromBody] CreateContainerDto dto)
        {
            var result = await _containerService.CreateContainerAsync(dto);
            return CreatedAtAction(nameof(GetContainerById), new { id = result.Id, version = "1.0" }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> UpdateContainer(Guid id, [FromBody] UpdateContainerDto dto)
        {
            var result = await _containerService.UpdateContainerAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> DeleteContainer(Guid id)
        {
            await _containerService.DeleteContainerAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/cargo")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> LinkCargo(Guid id, [FromBody] LinkCargoDto dto)
        {
            var result = await _containerService.LinkCargoAsync(id, dto.CargoId);
            return Ok(result);
        }

        [HttpDelete("{id}/cargo/{cargoId}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> UnlinkCargo(Guid id, Guid cargoId)
        {
            var result = await _containerService.UnlinkCargoAsync(id, cargoId);
            return Ok(result);
        }

        [HttpPost("{id}/tracking-events")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> RecordTrackingEvent(Guid id, [FromBody] RecordTrackingEventDto dto)
        {
            // Extract the user ID from the claims (ICurrentUserService is also an option, but Claims is straightforward)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized();
            }

            var result = await _containerService.RecordTrackingEventAsync(id, dto, userId);
            // Append-only, returning 201 Created but technically no GET individual event endpoint exists yet per specs,
            // so we don't provide a location header. Just the object.
            return StatusCode(201, result);
        }

        [HttpGet("{id}/tracking-events")]
        public async Task<IActionResult> GetTrackingEvents(Guid id)
        {
            var events = await _containerService.GetTrackingEventsAsync(id);
            return Ok(events);
        }
    }
}
