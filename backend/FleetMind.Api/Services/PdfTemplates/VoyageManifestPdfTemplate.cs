using System;
using System.Collections.Generic;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.DTOs.Reporting;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace FleetMind.Api.Services.PdfTemplates;

public class VoyageManifestPdfTemplate : IDocument
{
    private readonly VoyageDto _voyage;
    private readonly List<VoyageManifestReportRowDto> _manifestRows;

    public VoyageManifestPdfTemplate(VoyageDto voyage, List<VoyageManifestReportRowDto> manifestRows)
    {
        _voyage = voyage;
        _manifestRows = manifestRows;
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
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.Arial));

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
                column.Item().Text($"Voyage Manifest: {_voyage.VoyageNumber}")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                
                column.Item().PaddingTop(5).Text(text =>
                {
                    text.Span("Ship: ").SemiBold();
                    text.Span(_voyage.ShipName ?? "Unknown");
                });

                column.Item().Text(text =>
                {
                    text.Span("Route: ").SemiBold();
                    text.Span($"{_voyage.OriginPortName ?? "Unknown"} to {_voyage.DestinationPortName ?? "Unknown"}");
                });

                column.Item().Text(text =>
                {
                    text.Span("Schedule: ").SemiBold();
                    text.Span($"{_voyage.DepartureDate:yyyy-MM-dd} to {_voyage.EstimatedArrivalDate:yyyy-MM-dd}");
                });
                
                column.Item().Text(text =>
                {
                    text.Span("Status: ").SemiBold();
                    text.Span(_voyage.Status);
                });
            });
        });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            if (_manifestRows == null || _manifestRows.Count == 0)
            {
                column.Item().Padding(10).Background(Colors.Grey.Lighten3).Text("No cargo recorded for this voyage.")
                    .FontSize(12).Italic().FontColor(Colors.Grey.Darken2);
            }
            else
            {
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Description
                        columns.RelativeColumn(2); // Type
                        columns.RelativeColumn(2); // Weight
                        columns.RelativeColumn(2); // Declared Value
                        columns.RelativeColumn(2); // Container
                    });

                    table.Header(header =>
                    {
                        header.Cell().Element(CellStyle).Text("Description");
                        header.Cell().Element(CellStyle).Text("Type");
                        header.Cell().Element(CellStyle).AlignRight().Text("Weight (kg)");
                        header.Cell().Element(CellStyle).AlignRight().Text("Declared Value");
                        header.Cell().Element(CellStyle).Text("Container");

                        static IContainer CellStyle(IContainer c) => 
                            c.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                    });

                    foreach (var row in _manifestRows)
                    {
                        table.Cell().Element(CellStyle).Text(row.Description);
                        table.Cell().Element(CellStyle).Text(row.Type);
                        table.Cell().Element(CellStyle).AlignRight().Text($"{row.WeightKg:N0}");
                        table.Cell().Element(CellStyle).AlignRight().Text($"${row.DeclaredValue:N2}");
                        table.Cell().Element(CellStyle).Text(row.ContainerNumber ?? "Unassigned");

                        static IContainer CellStyle(IContainer c) => 
                            c.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                    }
                });
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
