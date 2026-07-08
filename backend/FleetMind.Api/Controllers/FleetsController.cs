using System;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize] // Any authenticated user can read
    public class FleetsController : ControllerBase
    {
        private readonly IFleetService _fleetService;
        private readonly IShipService _shipService;

        public FleetsController(IFleetService fleetService, IShipService shipService)
        {
            _fleetService = fleetService;
            _shipService = shipService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFleets([FromQuery] FleetQueryDto query)
        {
            var result = await _fleetService.GetFleetsAsync(query);
            return Ok(result);
        }

        // Note: Exporting the full unpaginated result set by setting PageSize to int.MaxValue
        // is a known scalability consideration. If the underlying dataset grows very large,
        // a future improvement could add a hard row cap or move to a background-job-based export.
        [HttpGet("export")]
        public async Task<IActionResult> ExportFleets([FromQuery] FleetQueryDto query, [FromServices] IExportService exportService, [FromQuery] string format = "csv")
        {
            query.PageSize = int.MaxValue;
            query.PageNumber = 1;
            var result = await _fleetService.GetFleetsAsync(query);
            var data = result.Items;

            if (format.ToLower() == "xlsx")
            {
                var bytes = await exportService.ExportToExcelAsync(data, "Fleets");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "fleets-export.xlsx");
            }
            
            var csvBytes = await exportService.ExportToCsvAsync(data);
            return File(csvBytes, "text/csv", "fleets-export.csv");
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<FleetDto>> GetFleetById(Guid id)
        {
            var fleet = await _fleetService.GetFleetByIdAsync(id);
            return Ok(fleet);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<FleetDto>> CreateFleet([FromBody] CreateFleetDto dto)
        {
            var fleet = await _fleetService.CreateFleetAsync(dto);
            return CreatedAtAction(nameof(GetFleetById), new { version = "1.0", id = fleet.Id }, fleet);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<FleetDto>> UpdateFleet(Guid id, [FromBody] UpdateFleetDto dto)
        {
            var fleet = await _fleetService.UpdateFleetAsync(id, dto);
            return Ok(fleet);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult> DeactivateFleet(Guid id)
        {
            await _fleetService.DeactivateFleetAsync(id);
            return NoContent();
        }

        [HttpGet("{fleetId:guid}/ships")]
        public async Task<ActionResult<PagedResultDto<ShipDto>>> GetShipsForFleet(Guid fleetId, [FromQuery] ShipQueryDto query)
        {
            query.FleetId = fleetId;
            var result = await _shipService.GetShipsAsync(query);
            return Ok(result);
        }
    }
}
