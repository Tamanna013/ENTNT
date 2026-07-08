using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.Services;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/crew")]
    [Authorize]
    public class CrewMembersController : ControllerBase
    {
        private readonly ICrewMemberService _crewMemberService;
        private readonly ICurrentUserService _currentUserService;

        public CrewMembersController(ICrewMemberService crewMemberService, ICurrentUserService currentUserService)
        {
            _crewMemberService = crewMemberService;
            _currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCrewMembers([FromQuery] CrewMemberQueryDto query)
        {
            var result = await _crewMemberService.GetCrewMembersAsync(query);
            return Ok(result);
        }

        // Note: Exporting the full unpaginated result set by setting PageSize to int.MaxValue
        // is a known scalability consideration. If the underlying dataset grows very large,
        // a future improvement could add a hard row cap or move to a background-job-based export.
        [HttpGet("export")]
        public async Task<IActionResult> ExportCrewMembers([FromQuery] CrewMemberQueryDto query, [FromServices] IExportService exportService, [FromQuery] string format = "csv")
        {
            query.PageSize = int.MaxValue;
            query.PageNumber = 1;
            var result = await _crewMemberService.GetCrewMembersAsync(query);
            var data = result.Items;

            if (format.ToLower() == "xlsx")
            {
                var bytes = await exportService.ExportToExcelAsync(data, "CrewMembers");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "crew-export.xlsx");
            }
            
            var csvBytes = await exportService.ExportToCsvAsync(data);
            return File(csvBytes, "text/csv", "crew-export.csv");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CrewMemberDto>> GetCrewMemberById(Guid id)
        {
            var result = await _crewMemberService.GetCrewMemberByIdAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult<CrewMemberDto>> CreateCrewMember([FromBody] CreateCrewMemberDto dto)
        {
            var result = await _crewMemberService.CreateCrewMemberAsync(dto);
            return CreatedAtAction(nameof(GetCrewMemberById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult<CrewMemberDto>> UpdateCrewMember(Guid id, [FromBody] UpdateCrewMemberDto dto)
        {
            var result = await _crewMemberService.UpdateCrewMemberAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult> DeactivateCrewMember(Guid id)
        {
            await _crewMemberService.DeactivateCrewMemberAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/assign")]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult<CrewMemberDto>> AssignToShip(Guid id, [FromBody] AssignToShipDto dto)
        {
            var result = await _crewMemberService.AssignToShipAsync(id, dto.ShipId);
            return Ok(result);
        }

        [HttpPut("{id}/unassign")]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult<CrewMemberDto>> UnassignFromShip(Guid id)
        {
            var result = await _crewMemberService.UnassignFromShipAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/certifications")]
        public async Task<ActionResult<System.Collections.Generic.List<CrewCertificationDto>>> GetCertifications(Guid id)
        {
            var result = await _crewMemberService.GetCertificationsAsync(id);
            return Ok(result);
        }

        [HttpPost("{id}/certifications")]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult<CrewCertificationDto>> UploadCertification(
            Guid id, 
            [FromForm] Microsoft.AspNetCore.Http.IFormFile file, 
            [FromForm] string certificationName, 
            [FromForm] DateOnly expiryDate)
        {
            var userId = _currentUserService.UserId ?? Guid.Empty;
            if (userId == Guid.Empty)
            {
                return Unauthorized();
            }

            var result = await _crewMemberService.UploadCertificationAsync(id, file, certificationName, expiryDate, userId);
            return CreatedAtAction(nameof(GetCertifications), new { id = id }, result);
        }

        [HttpDelete("{id}/certifications/{certId}")]
        [Authorize(Policy = "AdminOrCrewManager")]
        public async Task<ActionResult> DeleteCertification(Guid id, Guid certId)
        {
            await _crewMemberService.DeleteCertificationAsync(id, certId);
            return NoContent();
        }
    }
}
