using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Analytics;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/analytics")]
    [Authorize] // Base requirement: must be authenticated (reads open to all authenticated)
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly IPdfGenerationService _pdfGenerationService;
        private readonly IExportService _exportService;

        public AnalyticsController(
            IAnalyticsService analyticsService,
            IPdfGenerationService pdfGenerationService,
            IExportService exportService)
        {
            _analyticsService = analyticsService;
            _pdfGenerationService = pdfGenerationService;
            _exportService = exportService;
        }

        [HttpGet("fleet-summary")]
        public async Task<ActionResult<FleetSummaryAnalyticsDto>> GetFleetSummary()
        {
            var summary = await _analyticsService.GetFleetSummaryAsync();
            return Ok(summary);
        }

        [HttpGet("ship-utilization-trend")]
        public async Task<ActionResult<System.Collections.Generic.List<ShipUtilizationTrendPointDto>>> GetShipUtilizationTrend([FromQuery] int months = 12)
        {
            var trend = await _analyticsService.GetShipUtilizationTrendAsync(months);
            return Ok(trend);
        }

        [HttpGet("voyage-performance")]
        public async Task<ActionResult<System.Collections.Generic.List<VoyagePerformanceTrendPointDto>>> GetVoyagePerformanceTrend([FromQuery] int months = 12)
        {
            var trend = await _analyticsService.GetVoyagePerformanceTrendAsync(months);
            return Ok(trend);
        }

        [HttpGet("crew-compliance")]
        public async Task<ActionResult<System.Collections.Generic.List<CrewComplianceTrendPointDto>>> GetCrewComplianceTrend([FromQuery] int months = 12)
        {
            var trend = await _analyticsService.GetCrewComplianceTrendAsync(months);
            return Ok(trend);
        }

        [HttpGet("maintenance-cost-trend")]
        public async Task<ActionResult<System.Collections.Generic.List<MaintenanceCostTrendPointDto>>> GetMaintenanceCostTrend([FromQuery] int months = 12)
        {
            var trend = await _analyticsService.GetMaintenanceCostTrendAsync(months);
            return Ok(trend);
        }

        [HttpGet("financial-summary")]
        public async Task<ActionResult<System.Collections.Generic.List<FinancialSummaryTrendPointDto>>> GetFinancialSummaryTrend([FromQuery] int months = 12)
        {
            var data = await _analyticsService.GetFinancialSummaryTrendAsync(months);
            return Ok(data);
        }

        [HttpGet("ai-insights")]
        public async Task<ActionResult<AiSummaryResultDto>> GetAiInsights([FromQuery] int months = 12)
        {
            var data = await _analyticsService.GetAiInsightsAsync(months);
            return Ok(data);
        }

        [HttpGet("export-pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] int months = 12)
        {
            var bytes = await _pdfGenerationService.GenerateAnalyticsReportAsync(months);
            return File(bytes, "application/pdf", $"analytics-report-{months}mo.pdf");
        }

        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] int months = 12)
        {
            months = Math.Clamp(months, 1, 36);

            var fleetSummary = await _analyticsService.GetFleetSummaryAsync();
            var shipUtilization = await _analyticsService.GetShipUtilizationTrendAsync(months);
            var voyagePerformance = await _analyticsService.GetVoyagePerformanceTrendAsync(months);
            var crewCompliance = await _analyticsService.GetCrewComplianceTrendAsync(months);
            var maintenanceCost = await _analyticsService.GetMaintenanceCostTrendAsync(months);
            var financialSummary = await _analyticsService.GetFinancialSummaryTrendAsync(months);

            var sheets = new Dictionary<string, System.Collections.Generic.IEnumerable<object>>
            {
                { "Fleet Summary", new[] { fleetSummary } },
                { "Ship Utilization", shipUtilization },
                { "Voyage Performance", voyagePerformance },
                { "Crew Compliance", crewCompliance },
                { "Maintenance Cost", maintenanceCost },
                { "Financial Summary", financialSummary }
            };

            var bytes = await _exportService.ExportMultiSheetToExcelAsync(sheets);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"analytics-export-{months}mo.xlsx");
        }
    }
}
