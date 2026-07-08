using System;

namespace FleetMind.Api.DTOs.Containers
{
    public class UpdateContainerDto
    {
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public Guid? CurrentVoyageId { get; set; }
    }
}
