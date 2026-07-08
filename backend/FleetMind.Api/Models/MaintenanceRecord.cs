using System;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class MaintenanceRecord : BaseEntity
{
    public Guid ShipId { get; set; }
    public Ship Ship { get; set; } = null!;
    
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = MaintenanceStatus.Scheduled;
    public string Description { get; set; } = string.Empty;
    
    public DateTime ScheduledDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    
    public decimal EstimatedCost { get; set; }
    public decimal? ActualCost { get; set; }
    
    public string? PerformedBy { get; set; }
}
