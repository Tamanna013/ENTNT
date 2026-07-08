using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FleetMind.Api.Controllers.V1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize] // Base requirement: must be authenticated
    public class IncidentsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IPdfGenerationService _pdfGenerationService;

        public IncidentsController(
            IIncidentService incidentService,
            IAuthorizationService authorizationService,
            ICurrentUserService currentUserService,
            IPdfGenerationService pdfGenerationService)
        {
            _incidentService = incidentService;
            _authorizationService = authorizationService;
            _currentUserService = currentUserService;
            _pdfGenerationService = pdfGenerationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResultDto<IncidentDto>>> GetIncidents([FromQuery] IncidentQueryDto query)
        {
            var result = await _incidentService.GetIncidentsAsync(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentDto>> GetIncidentById(Guid id)
        {
            var result = await _incidentService.GetIncidentByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/ai-report")]
        public async Task<ActionResult<AiSummaryResultDto>> GetAiReport(Guid id)
        {
            var result = await _incidentService.GetAiReportAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        // DELIBERATELY NO POLICY HERE: Any authenticated user can report an incident
        public async Task<ActionResult<IncidentDto>> CreateIncident([FromBody] CreateIncidentDto dto)
        {
            var result = await _incidentService.CreateIncidentAsync(dto);
            return CreatedAtAction(nameof(GetIncidentById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentDto>> UpdateIncident(Guid id, [FromBody] UpdateIncidentDto dto)
        {
            var result = await _incidentService.UpdateIncidentAsync(id, dto);
            return Ok(result);
        }

        [HttpPut("{id}/status")]
        [Authorize(Policy = "AdminOrFleetManager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IncidentDto>> UpdateIncidentStatus(Guid id, [FromBody] UpdateIncidentStatusDto dto)
        {
            var result = await _incidentService.UpdateStatusAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteIncident(Guid id)
        {
            await _incidentService.DeleteIncidentAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/report-pdf")]
        public async Task<IActionResult> GetReportPdf(Guid id)
        {
            var bytes = await _pdfGenerationService.GenerateIncidentReportAsync(id);
            return File(bytes, "application/pdf", $"incident-report-{id}.pdf");
        }
    }
}
