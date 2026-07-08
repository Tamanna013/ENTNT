using System;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Common.Constants;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FleetMind.Api.Services.PdfTemplates;

public class IncidentReportPdfTemplate : IDocument
{
    private readonly IncidentDto _incident;
    private readonly string? _aiNarrative;

    public IncidentReportPdfTemplate(IncidentDto incident, string? aiNarrative = null)
    {
        _incident = incident;
        _aiNarrative = aiNarrative;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"Incident Report")
                    .FontSize(24).Bold().FontColor(Colors.Red.Darken2);
                
                column.Item().PaddingTop(10).Text(_incident.Title).FontSize(16).SemiBold();
                
                column.Item().PaddingTop(10).Row(detailsRow =>
                {
                    var severityColor = GetSeverityColor(_incident.Severity);
                    
                    detailsRow.AutoItem().PaddingRight(20).Text(text =>
                    {
                        text.Span("Severity: ").SemiBold();
                        text.Span(_incident.Severity).FontColor(severityColor).Bold();
                    });

                    detailsRow.AutoItem().PaddingRight(20).Text(text =>
                    {
                        text.Span("Status: ").SemiBold();
                        text.Span(_incident.Status);
                    });
                });
            });
        });
    }

    string GetSeverityColor(string severity)
    {
        return severity switch
        {
            IncidentSeverity.Critical => Colors.Red.Darken3,
            IncidentSeverity.High => Colors.Orange.Darken2,
            IncidentSeverity.Medium => Colors.Yellow.Darken3,
            IncidentSeverity.Low => Colors.Green.Darken2,
            _ => Colors.Black
        };
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Item().PaddingBottom(5).Text("Incident Details").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken3);
            
            column.Item().PaddingBottom(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(120);
                    columns.RelativeColumn();
                });

                table.Cell().Text("Reported By:").SemiBold();
                table.Cell().Text(_incident.ReportedByUserName ?? "Unknown");

                table.Cell().Text("Occurred At:").SemiBold();
                table.Cell().Text(_incident.OccurredAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                
                table.Cell().Text("Reported At:").SemiBold();
                table.Cell().Text(_incident.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss UTC"));

                table.Cell().Text("Ship:").SemiBold();
                table.Cell().Text(_incident.ShipName ?? "Unknown");

                if (!string.IsNullOrEmpty(_incident.VoyageNumber))
                {
                    table.Cell().Text("Voyage:").SemiBold();
                    table.Cell().Text(_incident.VoyageNumber);
                }
            });

            if (!string.IsNullOrWhiteSpace(_aiNarrative))
            {
                column.Item().PaddingBottom(5).Text("Narrative Summary").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken3);
                column.Item().PaddingBottom(20).Background(Colors.Blue.Lighten5).Padding(10).Text(_aiNarrative);
            }

            column.Item().PaddingBottom(5).Text("Description").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken3);
            column.Item().PaddingBottom(20).Background(Colors.Grey.Lighten4).Padding(10).Text(_incident.Description);

            if (_incident.ResolvedAt.HasValue)
            {
                column.Item().PaddingBottom(5).Text("Resolution").FontSize(14).SemiBold().FontColor(Colors.Grey.Darken3);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(120);
                        columns.RelativeColumn();
                    });

                    table.Cell().Text("Resolved At:").SemiBold();
                    table.Cell().Text(_incident.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                });
                
                if (!string.IsNullOrEmpty(_incident.ResolutionNotes))
                {
                    column.Item().PaddingTop(5).Background(Colors.Green.Lighten5).Padding(10).Text(_incident.ResolutionNotes);
                }
            }
        });
    }

    void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Generated on ");
            x.Span(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC"));
            x.Span(" - Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }
}
