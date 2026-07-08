using System.Collections.Generic;
using System.Text;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.DTOs.Voyages;

namespace FleetMind.Api.Services.Ai.PromptBuilders
{
    public class VoyageSummaryPromptBuilder
    {
        public string BuildSystemPrompt()
        {
            return "You are a maritime operations assistant. Summarize the following voyage information factually and concisely in 2-4 sentences. ONLY use the facts provided below. Do NOT invent, assume, or speculate about any detail not explicitly given - if information seems incomplete, simply omit it rather than filling gaps with plausible-sounding fabrications.";
        }

        public string BuildUserPrompt(VoyageDto voyage, List<CargoDto> cargoItems, List<IncidentDto> incidents)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Voyage Number: {voyage.VoyageNumber}");
            sb.AppendLine($"Ship Name: {voyage.ShipName}");
            sb.AppendLine($"Origin Port: {voyage.OriginPortName}");
            sb.AppendLine($"Destination Port: {voyage.DestinationPortName}");
            sb.AppendLine($"Departure Date: {voyage.DepartureDate:yyyy-MM-dd}");
            sb.AppendLine($"Estimated Arrival Date: {voyage.EstimatedArrivalDate:yyyy-MM-dd}");
            if (voyage.ActualArrivalDate.HasValue)
            {
                sb.AppendLine($"Actual Arrival Date: {voyage.ActualArrivalDate.Value:yyyy-MM-dd}");
            }
            sb.AppendLine($"Status: {voyage.Status}");
            
            sb.AppendLine("\nCargo:");
            if (cargoItems.Count == 0)
            {
                sb.AppendLine("No cargo items.");
            }
            else
            {
                sb.AppendLine($"{cargoItems.Count} cargo items:");
                foreach (var cargo in cargoItems)
                {
                    var hazardNote = string.IsNullOrWhiteSpace(cargo.HazardNotes) ? "" : " (with safety notes on file)";
                    sb.AppendLine($"- {cargo.Description}, type: {cargo.Type}, weight: {cargo.WeightKg}kg{hazardNote}");
                }
            }

            sb.AppendLine("\nIncidents:");
            if (incidents.Count == 0)
            {
                sb.AppendLine("No incidents reported.");
            }
            else
            {
                foreach (var incident in incidents)
                {
                    sb.AppendLine($"- {incident.Title} (Severity: {incident.Severity})");
                }
            }

            return sb.ToString();
        }
    }
}
