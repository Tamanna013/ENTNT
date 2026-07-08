using System;

namespace FleetMind.Api.DTOs.Maintenance;

public class CreateMaintenanceRecordDto
{
    public Guid ShipId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public string? PerformedBy { get; set; }
}
