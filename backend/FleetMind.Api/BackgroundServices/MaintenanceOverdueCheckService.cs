using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using FleetMind.Api.Services;
using FleetMind.Api.Configuration;

namespace FleetMind.Api.BackgroundServices;

public class MaintenanceOverdueCheckService : BackgroundService
{
    private readonly ILogger<MaintenanceOverdueCheckService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _checkInterval;

    public MaintenanceOverdueCheckService(
        ILogger<MaintenanceOverdueCheckService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<BackgroundServiceOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        
        // Default to 15 minutes if not configured
        var minutes = options.Value.MaintenanceOverdueCheckIntervalMinutes > 0 
            ? options.Value.MaintenanceOverdueCheckIntervalMinutes 
            : 15;
            
        _checkInterval = TimeSpan.FromMinutes(minutes);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MaintenanceOverdueCheckService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForOverdueMaintenanceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for overdue maintenance.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("MaintenanceOverdueCheckService is stopping.");
    }

    private async Task CheckForOverdueMaintenanceAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug("Running overdue maintenance check...");

        using var scope = _scopeFactory.CreateScope();
        var maintenanceService = scope.ServiceProvider.GetRequiredService<IMaintenanceRecordService>();

        var overdueIds = await maintenanceService.GetOverdueRecordIdsAsync();
        if (overdueIds.Count == 0)
        {
            _logger.LogDebug("No overdue maintenance records found.");
            return;
        }

        int flaggedCount = 0;
        foreach (var id in overdueIds)
        {
            if (stoppingToken.IsCancellationRequested) break;

            try
            {
                await maintenanceService.MarkOverdueAsync(id);
                flaggedCount++;
                _logger.LogInformation("Flagged maintenance record {Id} as Overdue.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to flag maintenance record {Id} as Overdue.", id);
            }
        }

        _logger.LogInformation("Overdue maintenance check completed. Flagged {Count} records.", flaggedCount);
    }
}
