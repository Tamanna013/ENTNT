using System;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FleetMind.Api.BackgroundServices
{
    public class AnalyticsCacheWarmupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AnalyticsCacheWarmupService> _logger;

        public AnalyticsCacheWarmupService(IServiceScopeFactory scopeFactory, ILogger<AnalyticsCacheWarmupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AnalyticsCacheWarmupService starting (one-shot run).");

            // Optional small delay to allow database seeding and initial startup to settle
            await Task.Delay(TimeSpan.FromSeconds(3), stoppingToken);

            using var scope = _scopeFactory.CreateScope();
            var analyticsService = scope.ServiceProvider.GetRequiredService<IAnalyticsService>();

            await TryWarmAsync(() => analyticsService.GetFleetSummaryAsync(), "FleetSummary", stoppingToken);
            await TryWarmAsync(() => analyticsService.GetShipUtilizationTrendAsync(12), "ShipUtilizationTrend", stoppingToken);
            await TryWarmAsync(() => analyticsService.GetVoyagePerformanceTrendAsync(12), "VoyagePerformanceTrend", stoppingToken);
            await TryWarmAsync(() => analyticsService.GetCrewComplianceTrendAsync(12), "CrewComplianceTrend", stoppingToken);
            await TryWarmAsync(() => analyticsService.GetMaintenanceCostTrendAsync(12), "MaintenanceCostTrend", stoppingToken);
            await TryWarmAsync(() => analyticsService.GetFinancialSummaryTrendAsync(12), "FinancialSummaryTrend", stoppingToken);

            _logger.LogInformation("AnalyticsCacheWarmupService completed its one-shot run.");
            // Do NOT loop here. We intentionally want this to run once and finish.
        }

        private async Task TryWarmAsync(Func<Task> warmupAction, string cacheKeyName, CancellationToken stoppingToken)
        {
            try
            {
                if (stoppingToken.IsCancellationRequested)
                    return;

                await warmupAction();
                _logger.LogInformation($"Successfully warmed up cache for {cacheKeyName}.");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to warm up cache for {cacheKeyName}. This is non-fatal; it will be computed on the first real request.");
            }
        }
    }
}
