using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Documents;

public class DocumentQueryDto : PaginationQueryDto
{
    public string? Category { get; set; }
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
}
