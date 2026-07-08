using System.Collections.Generic;
using System.Linq;
using System.Text;
using FleetMind.Api.DTOs.Analytics;

namespace FleetMind.Api.Services.Ai.PromptBuilders
{
    public class AnalyticsInsightsPromptBuilder
    {
        public string BuildSystemPrompt()
        {
            return "You are a maritime fleet operations analyst assistant. Summarize the KEY trends visible in the fleet analytics data provided below in 3-5 sentences. Describe WHAT the data shows (specific directions and magnitudes of change) - do NOT confidently assert WHY a trend occurred unless a clear, directly-supported causal relationship is evident from the data itself (for example, if maintenance cost variance and ship utilization data both changed in the same period, you may note they coincide, but do not claim one CAUSED the other without stronger evidence than simple co-occurrence). Focus on the most notable changes across the provided period rather than restating every data point.";
        }

        public string BuildUserPrompt(
            FleetSummaryAnalyticsDto summary,
            List<ShipUtilizationTrendPointDto> utilization,
            List<VoyagePerformanceTrendPointDto> voyagePerformance,
            List<CrewComplianceTrendPointDto> crewCompliance,
            List<MaintenanceCostTrendPointDto> maintenanceCost)
        {
            // Prompt-size management decision:
            // For longer series (many months), we summarize the trend down to just the FIRST point, 
            // LAST point, and any single MOST-EXTREME point (highest/lowest value) rather than listing every single month.
            // A future refinement could tune exactly how much detail to include per series if the model's output quality suggests it needs more context.
            
            var sb = new StringBuilder();
            sb.AppendLine("FLEET SUMMARY (Current Snapshot)");
            sb.AppendLine($"Total Fleets: {summary.TotalFleets}");
            sb.AppendLine($"Total Ships: {summary.TotalShips} ({summary.ActiveShips} Active)");
            sb.AppendLine($"Total Crew: {summary.TotalCrew} ({summary.AssignedCrew} Assigned)\n");

            sb.AppendLine("SHIP UTILIZATION TREND");
            AppendSummarizedTrend(sb, utilization, x => x.Month, x => x.UtilizationPercentage, "Utilization %");

            sb.AppendLine("\nVOYAGE PERFORMANCE TREND");
            AppendSummarizedTrend(sb, voyagePerformance, x => x.Month, x => x.OnTimePercentage, "On-Time %");

            sb.AppendLine("\nCREW COMPLIANCE TREND");
            AppendSummarizedTrend(sb, crewCompliance, x => x.Month, x => x.ComplianceRate, "Compliance %");

            sb.AppendLine("\nMAINTENANCE COST TREND");
            AppendSummarizedTrend(sb, maintenanceCost, x => x.Month, x => x.VariancePercentage, "Variance %");

            return sb.ToString();
        }

        private void AppendSummarizedTrend<T>(
            StringBuilder sb, 
            List<T> series, 
            System.Func<T, string> getLabel, 
            System.Func<T, decimal> getValue, 
            string metricName)
        {
            if (series == null || !series.Any())
            {
                sb.AppendLine("No data available.");
                return;
            }

            if (series.Count <= 3)
            {
                // If the series is short, just output all points
                foreach (var pt in series)
                {
                    sb.AppendLine($"- {getLabel(pt)}: {getValue(pt):F1} {metricName}");
                }
            }
            else
            {
                // Summarize: First, Last, and Extremes
                var first = series.First();
                var last = series.Last();

                var maxVal = series.Max(getValue);
                var minVal = series.Min(getValue);

                var maxPt = series.First(x => getValue(x) == maxVal);
                var minPt = series.First(x => getValue(x) == minVal);

                var summarizedPoints = new List<T> { first, last };
                if (!summarizedPoints.Contains(maxPt)) summarizedPoints.Add(maxPt);
                if (!summarizedPoints.Contains(minPt)) summarizedPoints.Add(minPt);

                // Re-sort chronologically based on their original index in the series
                var orderedPoints = summarizedPoints.OrderBy(p => series.IndexOf(p)).ToList();

                foreach (var pt in orderedPoints)
                {
                    sb.AppendLine($"- {getLabel(pt)}: {getValue(pt):F1} {metricName}");
                }
            }
        }
    }
}
