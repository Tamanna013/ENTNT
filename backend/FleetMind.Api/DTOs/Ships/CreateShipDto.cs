using System;

namespace FleetMind.Api.DTOs.Ships
{
    public class CreateShipDto
    {
        public Guid FleetId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IMO { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int YearBuilt { get; set; }
        public decimal GrossTonnage { get; set; }
        public string Flag { get; set; } = string.Empty;
    }
}
