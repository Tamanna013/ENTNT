using System;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Configuration;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.BackgroundServices;

public class DelayedVoyageCheckService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IOptions<BackgroundServiceOptions> _options;
    private readonly ILogger<DelayedVoyageCheckService> _logger;

    public DelayedVoyageCheckService(
        IServiceScopeFactory scopeFactory,
        IOptions<BackgroundServiceOptions> options,
        ILogger<DelayedVoyageCheckService> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("DelayedVoyageCheckService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var intervalMinutes = _options.Value.DelayedVoyageCheckIntervalMinutes;

                // Create a fresh scope per iteration
                using (var scope = _scopeFactory.CreateScope())
                {
                    var voyageService = scope.ServiceProvider.GetRequiredService<IVoyageService>();
                    var overdueIds = await voyageService.GetOverdueVoyageIdsAsync();

                    int transitionedCount = 0;

                    foreach (var id in overdueIds)
                    {
                        try
                        {
                            await voyageService.UpdateStatusAsync(id, new UpdateVoyageStatusDto { Status = VoyageStatus.Delayed });
                            transitionedCount++;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to transition overdue voyage {VoyageId} to Delayed status.", id);
                        }
                    }

                    _logger.LogInformation("DelayedVoyageCheckService cycle completed. Transitioned {Count} voyages to Delayed.", transitionedCount);
                }

                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // Normal termination
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred during the DelayedVoyageCheckService background cycle.");
                // Prevent tight loop in case of continuous failures
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
        
        _logger.LogInformation("DelayedVoyageCheckService is stopping.");
    }
}
