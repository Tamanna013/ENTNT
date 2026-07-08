using System.Text;
using FleetMind.Api.DTOs.Incidents;

namespace FleetMind.Api.Services.Ai.PromptBuilders
{
    public class IncidentReportPromptBuilder
    {
        public string BuildSystemPrompt()
        {
            return "You are a maritime operations assistant drafting a formal incident report narrative. Write 2-4 sentences summarizing the incident factually and professionally, suitable for inclusion in an official report document. ONLY use the facts explicitly provided below. Do NOT speculate about the cause of the incident, assign fault or blame, or suggest a resolution that hasn't already occurred, unless such information is explicitly given. If the incident is still unresolved, simply state its current status factually rather than implying an outcome.";
        }

        public string BuildUserPrompt(IncidentDto incident)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Title: {incident.Title}");
            sb.AppendLine($"Ship: {incident.ShipName}");
            if (!string.IsNullOrWhiteSpace(incident.VoyageNumber))
            {
                sb.AppendLine($"Voyage Number: {incident.VoyageNumber}");
            }
            sb.AppendLine($"Severity: {incident.Severity}");
            sb.AppendLine($"Status: {incident.Status}");
            sb.AppendLine($"Occurred At: {incident.OccurredAt:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine($"Description: {incident.Description}");

            if (incident.ResolvedAt.HasValue || !string.IsNullOrWhiteSpace(incident.ResolutionNotes))
            {
                sb.AppendLine("\n--- Resolution Details ---");
                if (incident.ResolvedAt.HasValue)
                {
                    sb.AppendLine($"Resolved At: {incident.ResolvedAt.Value:yyyy-MM-dd HH:mm} UTC");
                }
                if (!string.IsNullOrWhiteSpace(incident.ResolutionNotes))
                {
                    sb.AppendLine($"Resolution Notes: {incident.ResolutionNotes}");
                }
            }
            else
            {
                sb.AppendLine("\n--- Resolution Details ---");
                sb.AppendLine("Incident is currently unresolved.");
            }

            return sb.ToString();
        }
    }
}
