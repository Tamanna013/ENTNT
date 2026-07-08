using System;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.Services;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/maintenance")]
[Authorize]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceRecordService _maintenanceService;

    public MaintenanceController(IMaintenanceRecordService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<MaintenanceRecordDto>), 200)]
    public async Task<IActionResult> GetMaintenanceRecords([FromQuery] MaintenanceRecordQueryDto query)
    {
        var result = await _maintenanceService.GetMaintenanceRecordsAsync(query);
        return Ok(result);
    }

    // Note: Exporting the full unpaginated result set by setting PageSize to int.MaxValue
    // is a known scalability consideration. If the underlying dataset grows very large,
    // a future improvement could add a hard row cap or move to a background-job-based export.
    [HttpGet("export")]
    public async Task<IActionResult> ExportMaintenance([FromQuery] MaintenanceRecordQueryDto query, [FromServices] IExportService exportService, [FromQuery] string format = "csv")
    {
        query.PageSize = int.MaxValue;
        query.PageNumber = 1;
        var result = await _maintenanceService.GetMaintenanceRecordsAsync(query);
        var data = result.Items;

        if (format.ToLower() == "xlsx")
        {
            var bytes = await exportService.ExportToExcelAsync(data, "Maintenance");
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "maintenance-export.xlsx");
        }
        
        var csvBytes = await exportService.ExportToCsvAsync(data);
        return File(csvBytes, "text/csv", "maintenance-export.csv");
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMaintenanceRecordById(Guid id)
    {
        var record = await _maintenanceService.GetMaintenanceRecordByIdAsync(id);
        return Ok(record);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOrMaintenanceOfficer")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CreateMaintenanceRecord([FromBody] CreateMaintenanceRecordDto dto)
    {
        var record = await _maintenanceService.CreateMaintenanceRecordAsync(dto);
        return CreatedAtAction(nameof(GetMaintenanceRecordById), new { id = record.Id, version = "1.0" }, record);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOrMaintenanceOfficer")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMaintenanceRecord(Guid id, [FromBody] UpdateMaintenanceRecordDto dto)
    {
        var record = await _maintenanceService.UpdateMaintenanceRecordAsync(id, dto);
        return Ok(record);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOrMaintenanceOfficer")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteMaintenanceRecord(Guid id)
    {
        await _maintenanceService.DeleteMaintenanceRecordAsync(id);
        return NoContent();
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = "AdminOrMaintenanceOfficer")]
    [ProducesResponseType(typeof(MaintenanceRecordDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateMaintenanceStatus(Guid id, [FromBody] UpdateMaintenanceStatusDto dto)
    {
        var record = await _maintenanceService.UpdateStatusAsync(id, dto);
        return Ok(record);
    }
}
