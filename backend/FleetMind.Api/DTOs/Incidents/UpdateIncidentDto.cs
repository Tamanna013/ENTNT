namespace FleetMind.Api.DTOs.Incidents
{
    public class UpdateIncidentDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string? ResolutionNotes { get; set; }
    }
}
