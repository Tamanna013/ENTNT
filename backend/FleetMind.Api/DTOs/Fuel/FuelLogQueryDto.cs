using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Fuel
{
    public class FuelLogQueryDto : PaginationQueryDto
    {
        public Guid? ShipId { get; set; }
        public Guid? VoyageId { get; set; }
        public string? FuelType { get; set; }
    }
}
