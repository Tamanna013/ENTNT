using System;
using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Containers
{
    public class ContainerQueryDto : PaginationQueryDto
    {
        public string? Status { get; set; }
        public string? Type { get; set; }
        public Guid? VoyageId { get; set; }
    }
}
