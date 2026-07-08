using System;
using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Containers
{
    public class ContainerDto
    {
        public Guid Id { get; set; }
        public string ContainerNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? CurrentVoyageId { get; set; }
        public string? VoyageNumber { get; set; }
        public List<Guid> LinkedCargoIds { get; set; } = new List<Guid>();
        public DateTime CreatedAt { get; set; }
    }
}
