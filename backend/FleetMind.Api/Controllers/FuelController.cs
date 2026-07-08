using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Services;
using Asp.Versioning;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/fuel")]
    [Authorize]
    public class FuelController : ControllerBase
    {
        private readonly IFuelLogService _fuelLogService;

        public FuelController(IFuelLogService fuelLogService)
        {
            _fuelLogService = fuelLogService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<FuelLogDto>>> GetFuelLogs([FromQuery] FuelLogQueryDto query)
        {
            var result = await _fuelLogService.GetFuelLogsAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FuelLogDto>> GetFuelLogById(Guid id)
        {
            var result = await _fuelLogService.GetFuelLogByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<FuelLogDto>> CreateFuelLog(CreateFuelLogDto dto)
        {
            var result = await _fuelLogService.CreateFuelLogAsync(dto);
            return CreatedAtAction(nameof(GetFuelLogById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<FuelLogDto>> UpdateFuelLog(Guid id, UpdateFuelLogDto dto)
        {
            var result = await _fuelLogService.UpdateFuelLogAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> DeleteFuelLog(Guid id)
        {
            await _fuelLogService.DeleteFuelLogAsync(id);
            return NoContent();
        }
    }
}
