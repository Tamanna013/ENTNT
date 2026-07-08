using System.Threading.Tasks;
using FleetMind.Api.DTOs.Analytics;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services
{
    public interface IAnalyticsService
    {
        Task<FleetSummaryAnalyticsDto> GetFleetSummaryAsync();
        Task<System.Collections.Generic.List<ShipUtilizationTrendPointDto>> GetShipUtilizationTrendAsync(int months);
        Task<System.Collections.Generic.List<VoyagePerformanceTrendPointDto>> GetVoyagePerformanceTrendAsync(int months);
        Task<System.Collections.Generic.List<CrewComplianceTrendPointDto>> GetCrewComplianceTrendAsync(int months);
        Task<System.Collections.Generic.List<MaintenanceCostTrendPointDto>> GetMaintenanceCostTrendAsync(int months);
        Task<System.Collections.Generic.List<FinancialSummaryTrendPointDto>> GetFinancialSummaryTrendAsync(int months);
        Task<AiSummaryResultDto> GetAiInsightsAsync(int months);
    }
}
