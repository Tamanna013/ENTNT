using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Ships
{
    public class ShipQueryDto : PaginationQueryDto
    {
        public Guid? FleetId { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
    }
}
