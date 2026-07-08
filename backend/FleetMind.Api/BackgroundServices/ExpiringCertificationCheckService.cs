using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Configuration;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.BackgroundServices;

public class ExpiringCertificationCheckService : BackgroundService
{
    private readonly ILogger<ExpiringCertificationCheckService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly BackgroundServiceOptions _options;

    public ExpiringCertificationCheckService(
        ILogger<ExpiringCertificationCheckService> logger,
        IServiceScopeFactory scopeFactory,
        IOptions<BackgroundServiceOptions> options)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiringCertificationCheckService is starting.");
        
        var interval = TimeSpan.FromMinutes(_options.ExpiringCertificationCheckIntervalMinutes > 0 
            ? _options.ExpiringCertificationCheckIntervalMinutes 
            : 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("ExpiringCertificationCheckService running at: {Time}", DateTimeOffset.Now);

            try
            {
                await CheckExpiringCertificationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while checking for expiring certifications.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task CheckExpiringCertificationsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var recipientResolver = scope.ServiceProvider.GetRequiredService<INotificationRecipientResolver>();
        
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var inSevenDays = today.AddDays(7);

        // Find all active certifications expiring within the next 7 days
        var expiringCertifications = await unitOfWork.Context.Set<CrewCertification>()
            .Include(c => c.CrewMember)
            .Where(c => !c.IsDeleted && c.ExpiryDate >= today && c.ExpiryDate <= inSevenDays)
            .ToListAsync(stoppingToken);

        if (!expiringCertifications.Any()) return;
        
        var recipientIds = await recipientResolver.GetUserIdsByRolesAsync(NotificationType.CertificationExpiring, AppRoles.Admin, AppRoles.CrewManager);
        if (!recipientIds.Any()) return;

        int newNotificationsCount = 0;

        foreach (var cert in expiringCertifications)
        {
            foreach (var userId in recipientIds)
            {
                // Prevent duplicates: check if a notification already exists for this certification and this user
                bool alreadyNotified = await unitOfWork.Context.Notifications
                    .AnyAsync(n => n.RelatedEntityName == "CrewCertification" 
                                && n.RelatedEntityId == cert.Id 
                                && n.UserId == userId, 
                                stoppingToken);

                if (!alreadyNotified)
                {
                    var crewName = cert.CrewMember != null ? $"{cert.CrewMember.FirstName} {cert.CrewMember.LastName}" : "Unknown Crew Member";
                    
                    await notificationService.CreateAsync(
                        userId,
                        NotificationType.CertificationExpiring,
                        "Certification Expiring",
                        $"Crew member {crewName}'s certification '{cert.CertificationName}' is expiring on {cert.ExpiryDate:yyyy-MM-dd}.",
                        "CrewCertification",
                        cert.Id
                    );
                    newNotificationsCount++;
                }
            }
        }
        
        _logger.LogInformation("ExpiringCertificationCheckService cycle complete. Checked {Count} expiring certs. Created {NotifCount} new notifications.", expiringCertifications.Count, newNotificationsCount);
    }
}
