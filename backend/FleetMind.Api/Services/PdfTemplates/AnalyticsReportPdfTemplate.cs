using System;
using System.Collections.Generic;
using FleetMind.Api.DTOs.Analytics;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FleetMind.Api.Services.PdfTemplates;

public class AnalyticsReportPdfTemplate : IDocument
{
    private readonly FleetSummaryAnalyticsDto _fleetSummary;
    private readonly IEnumerable<ShipUtilizationTrendPointDto> _shipUtilization;
    private readonly IEnumerable<VoyagePerformanceTrendPointDto> _voyagePerformance;
    private readonly IEnumerable<CrewComplianceTrendPointDto> _crewCompliance;
    private readonly IEnumerable<MaintenanceCostTrendPointDto> _maintenanceCost;
    private readonly IEnumerable<FinancialSummaryTrendPointDto> _financialSummary;
    private readonly int _months;

    public AnalyticsReportPdfTemplate(
        FleetSummaryAnalyticsDto fleetSummary,
        IEnumerable<ShipUtilizationTrendPointDto> shipUtilization,
        IEnumerable<VoyagePerformanceTrendPointDto> voyagePerformance,
        IEnumerable<CrewComplianceTrendPointDto> crewCompliance,
        IEnumerable<MaintenanceCostTrendPointDto> maintenanceCost,
        IEnumerable<FinancialSummaryTrendPointDto> financialSummary,
        int months)
    {
        _fleetSummary = fleetSummary;
        _shipUtilization = shipUtilization;
        _voyagePerformance = voyagePerformance;
        _crewCompliance = crewCompliance;
        _maintenanceCost = maintenanceCost;
        _financialSummary = financialSummary;
        _months = months;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(50);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(10));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"FleetMind AI Analytics Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Trailing {_months} Months").FontSize(14);
                column.Item().PaddingTop(5).Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(1).Column(column =>
        {
            column.Spacing(20);

            // Fleet Summary KPIs
            column.Item().Text("Fleet Summary KPIs").FontSize(14).SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Total Fleets").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Ships (Active / Total)").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Crew (Assigned / Total)").SemiBold();
                });

                table.Cell().PaddingTop(5).Text(_fleetSummary.TotalFleets.ToString());
                table.Cell().PaddingTop(5).Text($"{_fleetSummary.ActiveShips} / {_fleetSummary.TotalShips}");
                table.Cell().PaddingTop(5).Text($"{_fleetSummary.AssignedCrew} / {_fleetSummary.TotalCrew}");
            });

            // Ship Utilization
            column.Item().Text("Ship Utilization Trend").FontSize(14).SemiBold();
            column.Item().Text("Note: Approximate - inferred from maintenance records, not a precise historical log.").FontSize(9).Italic().FontColor(Colors.Grey.Darken1);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Month").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Total Ships").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Active Ships").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Utilization").SemiBold();
                });

                foreach (var item in _shipUtilization)
                {
                    table.Cell().PaddingVertical(2).Text(item.Month);
                    table.Cell().PaddingVertical(2).Text(item.TotalShips.ToString());
                    table.Cell().PaddingVertical(2).Text(item.ActiveShips.ToString());
                    table.Cell().PaddingVertical(2).Text($"{item.UtilizationPercentage}%");
                }
            });

            // Voyage Performance
            column.Item().Text("Voyage Performance Trend").FontSize(14).SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Month").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Completed Voyages").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("On-Time Voyages").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("On-Time Rate").SemiBold();
                });

                foreach (var item in _voyagePerformance)
                {
                    table.Cell().PaddingVertical(2).Text(item.Month);
                    table.Cell().PaddingVertical(2).Text(item.CompletedVoyages.ToString());
                    table.Cell().PaddingVertical(2).Text(item.OnTimeVoyages.ToString());
                    table.Cell().PaddingVertical(2).Text($"{item.OnTimePercentage}%");
                }
            });

            // Crew Compliance
            column.Item().Text("Crew Certification Compliance Trend").FontSize(14).SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Month").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Active Certs").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Expired Count").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Compliance Rate").SemiBold();
                });

                foreach (var item in _crewCompliance)
                {
                    table.Cell().PaddingVertical(2).Text(item.Month);
                    table.Cell().PaddingVertical(2).Text(item.TotalActiveCertifications.ToString());
                    table.Cell().PaddingVertical(2).Text(item.ExpiredCount.ToString());
                    table.Cell().PaddingVertical(2).Text($"{item.ComplianceRate}%");
                }
            });

            // Maintenance Cost Variance
            column.Item().Text("Maintenance Cost Variance").FontSize(14).SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Month").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Estimated Cost").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Actual Cost").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Variance").SemiBold();
                });

                foreach (var item in _maintenanceCost)
                {
                    table.Cell().PaddingVertical(2).Text(item.Month);
                    table.Cell().PaddingVertical(2).Text($"${item.TotalEstimatedCost:N0}");
                    table.Cell().PaddingVertical(2).Text($"${item.TotalActualCost:N0}");
                    table.Cell().PaddingVertical(2).Text($"{item.VariancePercentage}%");
                }
            });

            // Financial Summary
            column.Item().Text("Financial Summary").FontSize(14).SemiBold();
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                table.Header(header =>
                {
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Month").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Fuel Cost").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Maintenance Cost").SemiBold();
                    header.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(5).Text("Total Operating").SemiBold();
                });

                foreach (var item in _financialSummary)
                {
                    table.Cell().PaddingVertical(2).Text(item.Month);
                    table.Cell().PaddingVertical(2).Text($"${item.FuelCost:N0}");
                    table.Cell().PaddingVertical(2).Text($"${item.MaintenanceCost:N0}");
                    table.Cell().PaddingVertical(2).Text($"${item.TotalOperatingCost:N0}");
                }
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }
}
