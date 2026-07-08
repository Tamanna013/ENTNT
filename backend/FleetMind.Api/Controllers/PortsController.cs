using System;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ports;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [Authorize]
    public class PortsController : ControllerBase
    {
        private readonly IPortService _portService;

        public PortsController(IPortService portService)
        {
            _portService = portService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<PortDto>>> GetPorts([FromQuery] PortQueryDto query)
        {
            /*
             * Caching Justification:
             * Ports represent genuinely slow-changing master/reference data. 
             * Caching this data client-side for a short window (5 minutes) is safe and reduces load.
             * NO OTHER controller in this project should receive similar caching headers without 
             * careful justification. Most application data (Voyages, Maintenance, Incidents, Notifications, etc.) 
             * is status-workflow-driven or user-specific, must always reflect current state, 
             * and would be actively HARMED by caching stale responses.
             */
            Response.Headers["Cache-Control"] = "public, max-age=300";

            var result = await _portService.GetPortsAsync(query);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<PortDto>> GetPortById(Guid id)
        {
            Response.Headers["Cache-Control"] = "public, max-age=300";
            
            var port = await _portService.GetPortByIdAsync(id);
            return Ok(port);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<PortDto>> CreatePort([FromBody] CreatePortDto dto)
        {
            var port = await _portService.CreatePortAsync(dto);
            return CreatedAtAction(nameof(GetPortById), new { version = "1.0", id = port.Id }, port);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<PortDto>> UpdatePort(Guid id, [FromBody] UpdatePortDto dto)
        {
            var port = await _portService.UpdatePortAsync(id, dto);
            return Ok(port);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult> DeactivatePort(Guid id)
        {
            await _portService.DeactivatePortAsync(id);
            return NoContent();
        }
    }
}
