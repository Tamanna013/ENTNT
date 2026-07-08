using System;

namespace FleetMind.Api.DTOs.Incidents
{
    public class UpdateIncidentStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionNotes { get; set; }
    }
}
