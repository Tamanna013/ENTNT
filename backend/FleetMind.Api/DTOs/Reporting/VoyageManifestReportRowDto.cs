using System;

namespace FleetMind.Api.DTOs.Reporting;

public class VoyageManifestReportRowDto
{
    public Guid CargoId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal WeightKg { get; set; }
    public decimal DeclaredValue { get; set; }
    public string ConsigneeName { get; set; } = string.Empty;
    public string? ContainerNumber { get; set; }
}
