using System;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.DTOs.Attachments;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class ShipsController : ControllerBase
    {
        private readonly IShipService _shipService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICrewMemberService _crewMemberService;
        private readonly IVoyageService _voyageService;
        private readonly IMaintenanceRecordService _maintenanceService;
        private readonly IFuelLogService _fuelLogService;
        private readonly IIncidentService _incidentService;

        public ShipsController(
            IShipService shipService, 
            ICurrentUserService currentUserService, 
            ICrewMemberService crewMemberService,
            IVoyageService voyageService,
            IMaintenanceRecordService maintenanceService,
            IFuelLogService fuelLogService,
            IIncidentService incidentService)
        {
            _shipService = shipService;
            _currentUserService = currentUserService;
            _crewMemberService = crewMemberService;
            _voyageService = voyageService;
            _maintenanceService = maintenanceService;
            _fuelLogService = fuelLogService;
            _incidentService = incidentService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ShipDto>>> GetShips([FromQuery] ShipQueryDto query)
        {
            var result = await _shipService.GetShipsAsync(query);
            return Ok(result);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ShipDto>> GetShipById(Guid id)
        {
            var ship = await _shipService.GetShipByIdAsync(id);
            return Ok(ship);
        }

        [HttpGet("{id:guid}/ai-maintenance-recommendations")]
        public async Task<ActionResult<AiRecommendationResultDto>> GetAiMaintenanceRecommendations(Guid id)
        {
            var result = await _shipService.GetAiMaintenanceRecommendationsAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<ShipDto>> CreateShip([FromBody] CreateShipDto dto)
        {
            var ship = await _shipService.CreateShipAsync(dto);
            return CreatedAtAction(nameof(GetShipById), new { version = "1.0", id = ship.Id }, ship);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<ShipDto>> UpdateShip(Guid id, [FromBody] UpdateShipDto dto)
        {
            var ship = await _shipService.UpdateShipAsync(id, dto);
            return Ok(ship);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult> DeactivateShip(Guid id)
        {
            await _shipService.DeactivateShipAsync(id);
            return NoContent();
        }

        [HttpGet("{id:guid}/crew")]
        public async Task<ActionResult<PagedResultDto<CrewMemberDto>>> GetCrewForShip(Guid id, [FromQuery] CrewMemberQueryDto query)
        {
            var result = await _crewMemberService.GetCrewForShipAsync(id, query);
            return Ok(result);
        }

        [HttpGet("{id:guid}/voyages")]
        public async Task<ActionResult<PagedResultDto<VoyageDto>>> GetVoyagesForShip(Guid id, [FromQuery] VoyageQueryDto query)
        {
            var result = await _voyageService.GetVoyagesForShipAsync(id, query);
            return Ok(result);
        }

        [HttpGet("{id:guid}/maintenance")]
        [ProducesResponseType(typeof(PagedResultDto<MaintenanceRecordDto>), 200)]
        public async Task<IActionResult> GetMaintenanceForShip(Guid id, [FromQuery] MaintenanceRecordQueryDto query)
        {
            var result = await _maintenanceService.GetMaintenanceForShipAsync(id, query);
            return Ok(result);
        }

        [HttpGet("{id:guid}/fuel-logs")]
        [ProducesResponseType(typeof(PagedResultDto<FuelLogDto>), 200)]
        public async Task<IActionResult> GetFuelLogsForShip(Guid id, [FromQuery] FuelLogQueryDto query)
        {
            var result = await _fuelLogService.GetFuelLogsForShipAsync(id, query);
            return Ok(result);
        }

        [HttpGet("{id:guid}/incidents")]
        [ProducesResponseType(typeof(PagedResultDto<IncidentDto>), 200)]
        public async Task<IActionResult> GetIncidentsForShip(Guid id, [FromQuery] IncidentQueryDto query)
        {
            var result = await _incidentService.GetIncidentsForShipAsync(id, query);
            return Ok(result);
        }

        [HttpGet("{id:guid}/attachments")]
        public async Task<ActionResult<System.Collections.Generic.List<AttachmentDto>>> GetShipAttachments(Guid id)
        {
            var result = await _shipService.GetAttachmentsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id:guid}/attachments")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<AttachmentDto>> UploadShipAttachment(Guid id, [FromForm] Microsoft.AspNetCore.Http.IFormFile file)
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var result = await _shipService.UploadAttachmentAsync(id, file, userId);
            return CreatedAtAction(nameof(GetShipById), new { version = "1.0", id = result.Id }, result);
        }

        [HttpPut("{id:guid}/primary-photo")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<ShipDto>> SetPrimaryPhoto(Guid id, [FromBody] SetPrimaryPhotoDto dto)
        {
            var ship = await _shipService.SetPrimaryPhotoAsync(id, dto.AttachmentId);
            return Ok(ship);
        }
    }
}
