using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Audit;

public class AuditLogQueryDto : PaginationQueryDto
{
    public string? EntityName { get; set; }
    public string? Action { get; set; }
    public Guid? UserId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}
