using System;

namespace FleetMind.Api.DTOs.Ships
{
    public class ShipDto
    {
        public Guid Id { get; set; }
        public Guid FleetId { get; set; }
        public string FleetName { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string IMO { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int YearBuilt { get; set; }
        public decimal GrossTonnage { get; set; }
        public string Flag { get; set; } = string.Empty;
        public string? PrimaryPhotoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
