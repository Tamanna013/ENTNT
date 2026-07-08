using System;

namespace FleetMind.Api.DTOs.Containers
{
    public class CreateContainerDto
    {
        public string ContainerNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Guid? CurrentVoyageId { get; set; }
    }
}
