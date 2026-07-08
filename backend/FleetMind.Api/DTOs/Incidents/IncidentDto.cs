using System;

namespace FleetMind.Api.DTOs.Incidents
{
    public class IncidentDto
    {
        public Guid Id { get; set; }
        public Guid ShipId { get; set; }
        public string ShipName { get; set; } = string.Empty;
        public Guid? VoyageId { get; set; }
        public string? VoyageNumber { get; set; }
        
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public Guid ReportedByUserId { get; set; }
        public string ReportedByUserName { get; set; } = string.Empty;

        public DateTime OccurredAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
        
        public DateTime CreatedAt { get; set; }
    }
}
