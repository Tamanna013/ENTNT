using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Incidents
{
    public class IncidentQueryDto : PaginationQueryDto
    {
        public Guid? ShipId { get; set; }
        public Guid? VoyageId { get; set; }
        public string? Status { get; set; }
        public string? Severity { get; set; }
    }
}
