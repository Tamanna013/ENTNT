using System;

namespace FleetMind.Api.DTOs.Maintenance;

public class UpdateMaintenanceRecordDto
{
    public string Description { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public decimal EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    public string? PerformedBy { get; set; }
}
