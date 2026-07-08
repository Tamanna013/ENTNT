using System;
using System.Threading.Tasks;
using FleetMind.Api.Configuration;
using FleetMind.Api.DTOs.Analytics;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Common.Constants;
using Microsoft.Extensions.Options;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Services.Ai.PromptBuilders;

namespace FleetMind.Api.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICacheService _cacheService;
        private readonly CacheOptions _cacheOptions;
        private readonly IAiProvider _aiProvider;

        public AnalyticsService(IUnitOfWork unitOfWork, ICacheService cacheService, IOptions<CacheOptions> cacheOptions, IAiProvider aiProvider)
        {
            _unitOfWork = unitOfWork;
            _cacheService = cacheService;
            _cacheOptions = cacheOptions.Value;
            _aiProvider = aiProvider;
        }

        public async Task<FleetSummaryAnalyticsDto> GetFleetSummaryAsync()
        {
            return await _cacheService.GetOrCreateAsync("analytics:fleet-summary", async () =>
            {
                var totalFleets = await _unitOfWork.Fleets.CountAsync();
                var totalShips = await _unitOfWork.Ships.CountAsync();
                var activeShips = await _unitOfWork.Ships.CountAsync(s => s.Status == ShipStatus.Active);
                var totalCrew = await _unitOfWork.CrewMembers.CountAsync();
                var assignedCrew = await _unitOfWork.CrewMembers.CountAsync(c => c.ShipId != null);

                return new FleetSummaryAnalyticsDto
                {
                    TotalFleets = totalFleets,
                    TotalShips = totalShips,
                    ActiveShips = activeShips,
                    TotalCrew = totalCrew,
                    AssignedCrew = assignedCrew,
                    GeneratedAt = DateTime.UtcNow
                };
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }

        /* 
         * PROMINENT WARNING:
         * This is an APPROXIMATION, not a precise historical reconstruction. Ship.Status only records CURRENT
         * state with no history log. We infer past 'inactive' months from MaintenanceRecord date overlaps, 
         * since that entity DOES have genuine historical date data, but this does not capture ships that were 
         * Decommissioned or Docked for reasons unrelated to a tracked maintenance record. A future improvement 
         * adding a genuine ShipStatusHistory audit table would allow precise historical reconstruction; until then, 
         * this endpoint provides directionally useful trend data built honestly from what the system actually 
         * records, not a false claim of precision.
         */
        public async Task<System.Collections.Generic.List<ShipUtilizationTrendPointDto>> GetShipUtilizationTrendAsync(int months)
        {
            var clampedMonths = Math.Clamp(months, 1, 36);

            return await _cacheService.GetOrCreateAsync($"analytics:ship-utilization-trend:{clampedMonths}", async () =>
            {
                var trend = new System.Collections.Generic.List<ShipUtilizationTrendPointDto>();
                
                var totalShips = await _unitOfWork.Ships.CountAsync();
                
                var maintenanceRecords = await _unitOfWork.MaintenanceRecords.GetAllAsync();
                var now = DateTime.UtcNow;

                for (int i = clampedMonths - 1; i >= 0; i--)
                {
                    var targetMonth = now.AddMonths(-i);
                    var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                    var inactiveShips = new System.Collections.Generic.HashSet<Guid>();

                    foreach (var record in maintenanceRecords)
                    {
                        var effectiveEnd = record.CompletedDate;
                        if (!effectiveEnd.HasValue && (record.Status == MaintenanceStatus.InProgress || record.Status == MaintenanceStatus.Overdue))
                        {
                            effectiveEnd = now;
                        }

                        if (effectiveEnd.HasValue)
                        {
                            if (record.ScheduledDate <= monthEnd && effectiveEnd.Value >= monthStart)
                            {
                                inactiveShips.Add(record.ShipId);
                            }
                        }
                    }

                    var activeShips = totalShips - inactiveShips.Count;
                    var utilPercent = totalShips == 0 ? 0m : Math.Round((decimal)activeShips / totalShips * 100, 1);

                    trend.Add(new ShipUtilizationTrendPointDto
                    {
                        Month = targetMonth.ToString("yyyy-MM"),
                        TotalShips = totalShips,
                        ActiveShips = activeShips,
                        UtilizationPercentage = utilPercent
                    });
                }

                return trend;
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }

        /*
         * DELIBERATE DESIGN CHOICE:
         * This endpoint groups throughput and on-time performance by the ActualArrivalDate (i.e. when a voyage 
         * actually FINISHED), rather than the DepartureDate (when it STARTED). This is because "voyages completed 
         * in month X" is the more natural reading of "throughput" for this metric. Both grouping strategies are 
         * individually defensible, but this endpoint deliberately commits to ActualArrivalDate for consistency.
         */
        public async Task<System.Collections.Generic.List<VoyagePerformanceTrendPointDto>> GetVoyagePerformanceTrendAsync(int months)
        {
            var clampedMonths = Math.Clamp(months, 1, 36);

            return await _cacheService.GetOrCreateAsync($"analytics:voyage-performance:{clampedMonths}", async () =>
            {
                var trend = new System.Collections.Generic.List<VoyagePerformanceTrendPointDto>();
                
                // Fetch completed voyages
                var completedVoyages = await _unitOfWork.Voyages.FindAsync(v => v.Status == VoyageStatus.Completed && v.ActualArrivalDate.HasValue);
                var now = DateTime.UtcNow;

                for (int i = clampedMonths - 1; i >= 0; i--)
                {
                    var targetMonth = now.AddMonths(-i);
                    var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                    var voyagesInMonth = new System.Collections.Generic.List<Voyage>();
                    foreach (var v in completedVoyages)
                    {
                        if (v.ActualArrivalDate!.Value >= monthStart && v.ActualArrivalDate.Value <= monthEnd)
                        {
                            voyagesInMonth.Add(v);
                        }
                    }

                    int countCompleted = voyagesInMonth.Count;
                    int countOnTime = 0;

                    foreach (var v in voyagesInMonth)
                    {
                        if (v.ActualArrivalDate!.Value <= v.EstimatedArrivalDate)
                        {
                            countOnTime++;
                        }
                    }

                    var onTimePercent = countCompleted == 0 ? 0m : Math.Round((decimal)countOnTime / countCompleted * 100, 1);

                    trend.Add(new VoyagePerformanceTrendPointDto
                    {
                        Month = targetMonth.ToString("yyyy-MM"),
                        CompletedVoyages = countCompleted,
                        OnTimeVoyages = countOnTime,
                        OnTimePercentage = onTimePercent
                    });
                }

                return trend;
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }

        /*
         * DELIBERATE DESIGN CHOICE:
         * The comparison point is the FIRST DAY OF THE HISTORICAL MONTH being computed, NOT today's date - 
         * this is what makes the result genuinely vary month to month, correctly reflecting compliance 
         * AS OF that point in time, rather than incorrectly showing every historical month's compliance 
         * identically (which would happen if compared against today's date for every iteration).
         */
        public async Task<System.Collections.Generic.List<CrewComplianceTrendPointDto>> GetCrewComplianceTrendAsync(int months)
        {
            var clampedMonths = Math.Clamp(months, 1, 36);

            return await _cacheService.GetOrCreateAsync($"analytics:crew-compliance:{clampedMonths}", async () =>
            {
                var trend = new System.Collections.Generic.List<CrewComplianceTrendPointDto>();
                
                var certifications = await _unitOfWork.Repository<CrewCertification>().GetAllAsync();
                var now = DateTime.UtcNow;

                for (int i = clampedMonths - 1; i >= 0; i--)
                {
                    var targetMonth = now.AddMonths(-i);
                    var monthStart = new DateOnly(targetMonth.Year, targetMonth.Month, 1);

                    int totalActive = 0;
                    int expiredCount = 0;

                    foreach (var cert in certifications)
                    {
                        if (cert.ExpiryDate >= monthStart)
                        {
                            totalActive++;
                        }
                        else
                        {
                            expiredCount++;
                        }
                    }

                    var total = totalActive + expiredCount;
                    var complianceRate = total == 0 ? 0m : Math.Round((decimal)totalActive / total * 100, 1);

                    trend.Add(new CrewComplianceTrendPointDto
                    {
                        Month = targetMonth.ToString("yyyy-MM"),
                        TotalActiveCertifications = totalActive,
                        ExpiredCount = expiredCount,
                        ComplianceRate = complianceRate
                    });
                }

                return trend;
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }

        public async Task<System.Collections.Generic.List<MaintenanceCostTrendPointDto>> GetMaintenanceCostTrendAsync(int months)
        {
            var clampedMonths = Math.Clamp(months, 1, 36);

            return await _cacheService.GetOrCreateAsync($"analytics:maintenance-cost-trend:{clampedMonths}", async () =>
            {
                var trend = new System.Collections.Generic.List<MaintenanceCostTrendPointDto>();
                
                var maintenanceRecords = await _unitOfWork.MaintenanceRecords.GetAllAsync();
                var now = DateTime.UtcNow;

                for (int i = clampedMonths - 1; i >= 0; i--)
                {
                    var targetMonth = now.AddMonths(-i);
                    var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                    decimal totalEstimated = 0m;
                    decimal totalActual = 0m;

                    foreach (var record in maintenanceRecords)
                    {
                        if (record.ScheduledDate >= monthStart && record.ScheduledDate <= monthEnd)
                        {
                            totalEstimated += record.EstimatedCost;
                        }

                        if (record.Status == MaintenanceStatus.Completed && record.CompletedDate.HasValue)
                        {
                            if (record.CompletedDate.Value >= monthStart && record.CompletedDate.Value <= monthEnd)
                            {
                                totalActual += record.ActualCost ?? 0m;
                            }
                        }
                    }

                    var variancePercent = totalEstimated == 0m ? 0m : Math.Round((totalActual - totalEstimated) / totalEstimated * 100, 1);

                    trend.Add(new MaintenanceCostTrendPointDto
                    {
                        Month = targetMonth.ToString("yyyy-MM"),
                        TotalEstimatedCost = totalEstimated,
                        TotalActualCost = totalActual,
                        VariancePercentage = variancePercent
                    });
                }

                return trend;
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }

        /*
         * DELIBERATE DESIGN CHOICE:
         * This composition creates a SEPARATE cache entry from the underlying GetMaintenanceCostTrendAsync call 
         * (i.e., there are now two cached copies of related maintenance-cost data under different cache keys).
         * This is a deliberate, accepted simplicity-over-cache-minimalism tradeoff, not an oversight.
         * A more cache-storage-efficient implementation could share a single underlying computation, but would 
         * meaningfully complicate the clean GetOrCreateAsync pattern established for every other analytics endpoint 
         * in this phase, for a benefit (marginally less redundant cached data) that doesn't matter at this 
         * project's scale.
         */
        public async Task<System.Collections.Generic.List<FinancialSummaryTrendPointDto>> GetFinancialSummaryTrendAsync(int months)
        {
            var clampedMonths = Math.Clamp(months, 1, 36);

            return await _cacheService.GetOrCreateAsync($"analytics:financial-summary:{clampedMonths}", async () =>
            {
                var trend = new System.Collections.Generic.List<FinancialSummaryTrendPointDto>();
                
                var maintenanceTrend = await GetMaintenanceCostTrendAsync(clampedMonths);
                var fuelLogs = await _unitOfWork.Repository<FuelLog>().GetAllAsync();
                
                var now = DateTime.UtcNow;

                for (int i = clampedMonths - 1; i >= 0; i--)
                {
                    var targetMonth = now.AddMonths(-i);
                    var monthString = targetMonth.ToString("yyyy-MM");
                    
                    var monthStart = new DateTime(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                    decimal fuelCost = 0m;
                    foreach (var log in fuelLogs)
                    {
                        if (log.RecordedDate >= monthStart && log.RecordedDate <= monthEnd)
                        {
                            fuelCost += log.QuantityLiters * log.CostPerLiter;
                        }
                    }

                    var maintenancePoint = maintenanceTrend.Find(m => m.Month == monthString);
                    decimal maintenanceCost = maintenancePoint?.TotalActualCost ?? 0m;

                    trend.Add(new FinancialSummaryTrendPointDto
                    {
                        Month = monthString,
                        FuelCost = fuelCost,
                        MaintenanceCost = maintenanceCost,
                        TotalOperatingCost = fuelCost + maintenanceCost
                    });
                }

                return trend;
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }

        public async Task<AiSummaryResultDto> GetAiInsightsAsync(int months)
        {
            var clampedMonths = Math.Clamp(months, 1, 36);

            if (!_aiProvider.IsAvailable)
            {
                return new AiSummaryResultDto
                {
                    IsAvailable = false,
                    Summary = string.Empty,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            // We cache this endpoint precisely because it makes a real external AI API call each time, 
            // which has real latency/cost, unlike the underlying analytics data which is cheap to recompute.
            return await _cacheService.GetOrCreateAsync($"analytics:ai-insights:{clampedMonths}", async () =>
            {
                // This composition calls the EXISTING methods DIRECTLY, 
                // benefiting from their individual caching layers without needing a separate cache-warming concern.
                var summary = await GetFleetSummaryAsync();
                var utilTrend = await GetShipUtilizationTrendAsync(clampedMonths);
                var perfTrend = await GetVoyagePerformanceTrendAsync(clampedMonths);
                var compTrend = await GetCrewComplianceTrendAsync(clampedMonths);
                var costTrend = await GetMaintenanceCostTrendAsync(clampedMonths);

                var builder = new AnalyticsInsightsPromptBuilder();

                var result = await _aiProvider.CompleteAsync(new Common.AiCompletionRequest
                {
                    SystemPrompt = builder.BuildSystemPrompt(),
                    UserPrompt = builder.BuildUserPrompt(summary, utilTrend, perfTrend, compTrend, costTrend)
                });

                if (result.IsSuccess)
                {
                    return new AiSummaryResultDto
                    {
                        IsAvailable = true,
                        Summary = result.Content,
                        GeneratedAt = DateTime.UtcNow
                    };
                }

                return new AiSummaryResultDto
                {
                    IsAvailable = false,
                    Summary = string.Empty,
                    GeneratedAt = DateTime.UtcNow
                };
            }, TimeSpan.FromMinutes(_cacheOptions.AnalyticsExpirationMinutes));
        }
    }
}
