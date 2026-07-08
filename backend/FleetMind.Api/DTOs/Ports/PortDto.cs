using System;

namespace FleetMind.Api.DTOs.Ports
{
    public class PortDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string UnLocode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
