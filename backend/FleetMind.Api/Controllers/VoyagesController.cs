using System;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize]
    public class VoyagesController : ControllerBase
    {
        private readonly IVoyageService _voyageService;
        private readonly ICargoService _cargoService;
        private readonly ILogger<VoyagesController> _logger;
        private readonly IPdfGenerationService _pdfGenerationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentUserService _currentUserService;

        public VoyagesController(
            IVoyageService voyageService,
            ICargoService cargoService,
            IAuthorizationService authorizationService,
            ICurrentUserService currentUserService,
            ILogger<VoyagesController> logger,
            IPdfGenerationService pdfGenerationService)
        {
            _voyageService = voyageService;
            _cargoService = cargoService;
            _authorizationService = authorizationService;
            _currentUserService = currentUserService;
            _logger = logger;
            _pdfGenerationService = pdfGenerationService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<VoyageDto>), 200)]
        public async Task<IActionResult> GetVoyages([FromQuery] VoyageQueryDto query)
        {
            var result = await _voyageService.GetVoyagesAsync(query);
            return Ok(result);
        }

        // Note: Exporting the full unpaginated result set by setting PageSize to int.MaxValue
        // is a known scalability consideration. If the underlying dataset grows very large,
        // a future improvement could add a hard row cap or move to a background-job-based export.
        [HttpGet("export")]
        public async Task<IActionResult> ExportVoyages([FromQuery] VoyageQueryDto query, [FromServices] IExportService exportService, [FromQuery] string format = "csv")
        {
            query.PageSize = int.MaxValue;
            query.PageNumber = 1;
            var result = await _voyageService.GetVoyagesAsync(query);
            var data = result.Items;

            if (format.ToLower() == "xlsx")
            {
                var bytes = await exportService.ExportToExcelAsync(data, "Voyages");
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "voyages-export.xlsx");
            }
            
            var csvBytes = await exportService.ExportToCsvAsync(data);
            return File(csvBytes, "text/csv", "voyages-export.csv");
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<VoyageDto>> GetVoyageById(Guid id)
        {
            var voyage = await _voyageService.GetVoyageByIdAsync(id);
            return Ok(voyage);
        }

        [HttpGet("{id}/ai-summary")]
        public async Task<ActionResult<AiSummaryResultDto>> GetAiSummary(Guid id)
        {
            var result = await _voyageService.GetAiSummaryAsync(id);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<VoyageDto>> CreateVoyage([FromBody] CreateVoyageDto dto)
        {
            var voyage = await _voyageService.CreateVoyageAsync(dto);
            return CreatedAtAction(nameof(GetVoyageById), new { version = "1.0", id = voyage.Id }, voyage);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<VoyageDto>> UpdateVoyage(Guid id, [FromBody] UpdateVoyageDto dto)
        {
            var voyage = await _voyageService.UpdateVoyageAsync(id, dto);
            return Ok(voyage);
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult> DeleteVoyage(Guid id)
        {
            await _voyageService.DeleteVoyageAsync(id);
            return NoContent();
        }

        [HttpGet("{id}/manifest-pdf")]
        public async Task<IActionResult> GetManifestPdf(Guid id)
        {
            var bytes = await _pdfGenerationService.GenerateVoyageManifestAsync(id);
            var voyage = await _voyageService.GetVoyageByIdAsync(id);
            var filename = $"voyage-manifest-{voyage.VoyageNumber}.pdf";
            return File(bytes, "application/pdf", filename);
        }

        [HttpPut("{id:guid}/status")]
        [Authorize(Policy = "AdminOrFleetManager")]
        public async Task<ActionResult<VoyageDto>> UpdateVoyageStatus(Guid id, [FromBody] UpdateVoyageStatusDto dto)
        {
            var voyage = await _voyageService.UpdateStatusAsync(id, dto);
            return Ok(voyage);
        }

        [HttpGet("{id:guid}/cargo")]
        public async Task<ActionResult<PagedResultDto<CargoDto>>> GetCargoForVoyage(Guid id, [FromQuery] CargoQueryDto query)
        {
            var result = await _cargoService.GetCargoForVoyageAsync(id, query);
            return Ok(result);
        }
    }
}
