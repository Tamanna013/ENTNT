using System;
using System.Threading.Tasks;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Services.PdfTemplates;
using QuestPDF.Fluent;

namespace FleetMind.Api.Services;

public class PdfGenerationService : IPdfGenerationService
{
    private readonly IVoyageService _voyageService;
    private readonly IIncidentService _incidentService;
    private readonly IReportingService _reportingService;
    private readonly IAnalyticsService _analyticsService;

    public PdfGenerationService(
        IVoyageService voyageService,
        IIncidentService incidentService,
        IReportingService reportingService,
        IAnalyticsService analyticsService)
    {
        _voyageService = voyageService;
        _incidentService = incidentService;
        _reportingService = reportingService;
        _analyticsService = analyticsService;
    }

    public async Task<byte[]> GenerateAnalyticsReportAsync(int months)
    {
        months = Math.Clamp(months, 1, 36);

        var fleetSummary = await _analyticsService.GetFleetSummaryAsync();
        var shipUtilization = await _analyticsService.GetShipUtilizationTrendAsync(months);
        var voyagePerformance = await _analyticsService.GetVoyagePerformanceTrendAsync(months);
        var crewCompliance = await _analyticsService.GetCrewComplianceTrendAsync(months);
        var maintenanceCost = await _analyticsService.GetMaintenanceCostTrendAsync(months);
        var financialSummary = await _analyticsService.GetFinancialSummaryTrendAsync(months);

        var template = new AnalyticsReportPdfTemplate(
            fleetSummary,
            shipUtilization,
            voyagePerformance,
            crewCompliance,
            maintenanceCost,
            financialSummary,
            months);

        return template.GeneratePdf();
    }

    public async Task<byte[]> GenerateVoyageManifestAsync(Guid voyageId)
    {
        var voyage = await _voyageService.GetVoyageByIdAsync(voyageId);
        if (voyage == null)
            throw new NotFoundException("Voyage", voyageId);

        var manifestRows = await _reportingService.GetVoyageManifestReportAsync(voyageId);

        var document = new VoyageManifestPdfTemplate(voyage, manifestRows);
        return document.GeneratePdf();
    }

    public async Task<byte[]> GenerateIncidentReportAsync(Guid incidentId)
    {
        var incident = await _incidentService.GetIncidentByIdAsync(incidentId);
        if (incident == null)
            throw new NotFoundException("Incident", incidentId);

        var aiReportResult = await _incidentService.GetAiReportAsync(incidentId);
        var aiNarrative = aiReportResult.IsAvailable ? aiReportResult.Summary : null;

        var document = new IncidentReportPdfTemplate(incident, aiNarrative);
        return document.GeneratePdf();
    }
}
