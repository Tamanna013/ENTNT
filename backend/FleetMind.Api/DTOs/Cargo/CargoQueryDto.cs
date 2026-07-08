using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Cargo
{
    public class CargoQueryDto : PaginationQueryDto
    {
        public Guid? VoyageId { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
    }
}
