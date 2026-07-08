using System;

namespace FleetMind.Api.DTOs.Maintenance;

public class MaintenanceRecordDto
{
    public Guid Id { get; set; }
    public Guid ShipId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? PerformedBy { get; set; }
    public DateTime CreatedAt { get; set; }
}
