using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Maintenance;

public class MaintenanceRecordQueryDto : PaginationQueryDto
{
    public Guid? ShipId { get; set; }
    public string? Status { get; set; }
    public string? Type { get; set; }
    public DateTime? DueBefore { get; set; }
}
