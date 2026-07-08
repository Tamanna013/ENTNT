using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Notifications;
using FleetMind.Api.Models;

namespace FleetMind.Api.Services;

public interface INotificationService
{
    Task<Notification> CreateAsync(
        Guid userId, 
        string type, 
        string title, 
        string message, 
        string? relatedEntityName = null, 
        Guid? relatedEntityId = null);

    Task<PagedResultDto<NotificationDto>> GetForUserAsync(Guid userId, NotificationQueryDto query);
    
    Task<int> GetUnreadCountAsync(Guid userId);
    
    Task MarkReadAsync(Guid userId, Guid notificationId);
    
    Task MarkAllReadAsync(Guid userId);
}
