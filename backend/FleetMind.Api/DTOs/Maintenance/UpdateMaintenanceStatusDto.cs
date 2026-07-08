using System;

namespace FleetMind.Api.DTOs.Maintenance;

public class UpdateMaintenanceStatusDto
{
    public string Status { get; set; } = string.Empty;
    public decimal? ActualCost { get; set; }
    public DateTime? CompletedDate { get; set; }
}
