using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Crew
{
    public class CrewMemberQueryDto : PaginationQueryDto
    {
        public Guid? ShipId { get; set; }
        public string? Status { get; set; }
        public string? Rank { get; set; }
        public bool? Unassigned { get; set; }
    }
}
