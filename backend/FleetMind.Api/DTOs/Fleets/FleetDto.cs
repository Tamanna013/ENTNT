using System;

namespace FleetMind.Api.DTOs.Fleets
{
    public class FleetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid HomePortId { get; set; }
        public string HomePortName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        
        // Hardcoded to 0 for now. Will be populated once the Ship entity exists in a later milestone.
        public int ShipCount { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
