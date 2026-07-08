using System.Collections.Generic;
using System.Text;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.DTOs.Ships;

namespace FleetMind.Api.Services.Ai.PromptBuilders
{
    public class MaintenanceRecommendationPromptBuilder
    {
        public string BuildSystemPrompt()
        {
            return "You are a maritime maintenance planning assistant. Based on the ship's historical maintenance record patterns provided below, suggest 2-4 SHORT, SPECIFIC considerations for upcoming maintenance planning - for example, noting if a particular maintenance type appears to recur on a roughly regular interval and when the next occurrence might reasonably be expected based on that pattern. Frame every suggestion as an INFORMATIONAL OBSERVATION, never as a directive or certainty (use language like 'may be due for consideration' rather than 'must be scheduled'). If the provided history is TOO SPARSE to identify any meaningful pattern (very few records, or no clear recurring intervals), respond with a SINGLE item stating that insufficient historical data exists to generate meaningful recommendations at this time - do NOT fabricate confident-sounding suggestions from insufficient data. Format your response as a simple list, one suggestion per line, with no additional preamble or closing remarks.";
        }

        public string BuildUserPrompt(ShipDto ship, List<MaintenanceRecordDto> maintenanceHistory)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Ship: {ship.Name} ({ship.Type})");
            sb.AppendLine("\nCompleted Maintenance History:");
            
            if (maintenanceHistory == null || maintenanceHistory.Count == 0)
            {
                sb.AppendLine("No completed maintenance records available.");
            }
            else
            {
                foreach (var record in maintenanceHistory)
                {
                    sb.AppendLine($"- Type: {record.Type}, Scheduled: {record.ScheduledDate:yyyy-MM-dd}, Completed: {record.CompletedDate:yyyy-MM-dd}, Description: {record.Description}");
                }
            }

            return sb.ToString();
        }
    }
}
