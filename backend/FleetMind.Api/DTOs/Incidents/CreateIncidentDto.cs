using System;

namespace FleetMind.Api.DTOs.Incidents
{
    public class CreateIncidentDto
    {
        public Guid ShipId { get; set; }
        public Guid? VoyageId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public DateTime OccurredAt { get; set; }
    }
}
