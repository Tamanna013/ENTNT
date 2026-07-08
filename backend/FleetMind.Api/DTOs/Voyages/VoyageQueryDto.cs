using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Voyages
{
    public class VoyageQueryDto : PaginationQueryDto
    {
        public Guid? ShipId { get; set; }
        public string? Status { get; set; }
        public DateTime? DepartureFrom { get; set; }
        public DateTime? DepartureTo { get; set; }
    }
}
