using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/cargo")]
    [ApiVersion("1.0")]
    [Authorize]
    public class CargoController : ControllerBase
    {
        private readonly ICargoService _cargoService;

        public CargoController(ICargoService cargoService)
        {
            _cargoService = cargoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCargoItems([FromQuery] CargoQueryDto query)
        {
            var result = await _cargoService.GetCargoItemsAsync(query);
            return Ok(result);
        }

        // Note: Exporting the full unpaginated result set by setting PageSize to int.MaxValue
        // is a known scalability consideration. If the underlying dataset grows very large,
        // a future improvement could add a hard row cap or move to a background-job-based export.
        [HttpGet("export")]
        public async Task<IActionResult> ExportCargo([FromQuery] CargoQueryDto query, [FromServices] IExportService exportService, [FromQuery] string format = "csv")
        {
            query.PageSize = int.MaxValue;
            query.PageNumber = 1;
            var result = await _cargoService.GetCargoItemsAsync(query);
            var data = result.Items;

            if (format.ToLower() == "xlsx")
            {
                var bytes = await exportService.ExportToExcelAsync(data, "Cargo");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "cargo-export.xlsx");
            }
            
            var csvBytes = await exportService.ExportToCsvAsync(data);
            return File(csvBytes, "text/csv", "cargo-export.csv");
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CargoDto>> GetCargoById(Guid id)
        {
            var cargo = await _cargoService.GetCargoByIdAsync(id);
            return Ok(cargo);
        }

        [HttpGet("{id:guid}/ai-risk-assessment")]
        public async Task<ActionResult<AiRecommendationResultDto>> GetAiRiskAssessment(Guid id)
        {
            var result = await _cargoService.GetAiRiskAssessmentAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> CreateCargo([FromBody] CreateCargoDto dto)
        {
            var result = await _cargoService.CreateCargoAsync(dto);
            return CreatedAtAction(nameof(GetCargoById), new { id = result.Id, version = "1.0" }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> UpdateCargo(Guid id, [FromBody] UpdateCargoDto dto)
        {
            var result = await _cargoService.UpdateCargoAsync(id, dto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<IActionResult> DeleteCargo(Guid id)
        {
            await _cargoService.DeleteCargoAsync(id);
            return NoContent();
        }
    }
}
