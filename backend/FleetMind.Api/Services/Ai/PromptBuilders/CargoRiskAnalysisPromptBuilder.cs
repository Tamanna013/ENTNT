using System.Text;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Voyages;

namespace FleetMind.Api.Services.Ai.PromptBuilders
{
    public class CargoRiskAnalysisPromptBuilder
    {
        public string BuildSystemPrompt()
        {
            return "You are a maritime logistics risk-awareness assistant. Based on the cargo information provided below, note 2-4 SHORT, FACTUAL observations relevant to risk awareness for this shipment - for example, noting the cargo type's general handling considerations, or the relationship between a high declared value and appropriate care/documentation, or, for hazardous cargo specifically, restating (not elaborating beyond) whatever hazard notes are already on file. You are NOT providing a safety clearance, regulatory compliance determination, or any form of official risk sign-off - your output is purely informational awareness, never a substitute for qualified safety personnel, actual regulatory documentation, or established hazardous materials handling procedures. NEVER state or imply that a shipment is 'safe' or 'cleared' - only note factual characteristics relevant to awareness. If the cargo type is general/non-hazardous with a modest declared value, it's appropriate to note that risk considerations are minimal for this shipment, without inventing concerns where none are indicated by the data. Format your response as a simple list, one observation per line, with no additional preamble or closing remarks.";
        }

        public string BuildUserPrompt(CargoDto cargo, VoyageDto voyage)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Cargo Description: {cargo.Description}");
            sb.AppendLine($"Type: {cargo.Type}");
            sb.AppendLine($"Weight: {cargo.WeightKg} kg");
            sb.AppendLine($"Declared Value: {cargo.DeclaredValue:C}");
            sb.AppendLine($"Status: {cargo.Status}");
            
            if (!string.IsNullOrWhiteSpace(cargo.HazardNotes))
            {
                sb.AppendLine($"Hazard Notes: {cargo.HazardNotes}");
            }

            if (voyage != null)
            {
                var duration = voyage.EstimatedArrivalDate - voyage.DepartureDate;
                sb.AppendLine($"\nVoyage Context:");
                sb.AppendLine($"Origin: {voyage.OriginPortName}");
                sb.AppendLine($"Destination: {voyage.DestinationPortName}");
                sb.AppendLine($"Estimated Duration: {duration.TotalDays:F1} days");
            }

            return sb.ToString();
        }
    }
}
