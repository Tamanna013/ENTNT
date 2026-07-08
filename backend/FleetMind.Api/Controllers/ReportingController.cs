using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Reporting;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportingController : ControllerBase
{
    private readonly IReportingService _reportingService;

    public ReportingController(IReportingService reportingService)
    {
        _reportingService = reportingService;
    }

    [HttpGet("fleet-utilization")]
    public async Task<ActionResult<List<FleetUtilizationReportRowDto>>> GetFleetUtilizationReport()
    {
        var result = await _reportingService.GetFleetUtilizationReportAsync();
        return Ok(result);
    }

    [HttpGet("voyage-manifest/{voyageId}")]
    public async Task<ActionResult<List<VoyageManifestReportRowDto>>> GetVoyageManifestReport(Guid voyageId)
    {
        var result = await _reportingService.GetVoyageManifestReportAsync(voyageId);
        return Ok(result);
    }

    [HttpGet("fuel-efficiency")]
    public async Task<ActionResult<List<FuelEfficiencyReportRowDto>>> GetFuelEfficiencyReport([FromQuery] int trailingDays = 90)
    {
        var result = await _reportingService.GetFuelEfficiencyReportAsync(trailingDays);
        return Ok(result);
    }
}
