using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Notifications;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Services;

public class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IGenericRepository<Notification> _repository;

    public NotificationService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        
        // We use the generic repository directly because Notification query needs are very simple,
        // and we want to avoid creating a dedicated repository when it isn't strictly necessary.
        _repository = _unitOfWork.Repository<Notification>();
    }

    public async Task<Notification> CreateAsync(
        Guid userId, 
        string type, 
        string title, 
        string message, 
        string? relatedEntityName = null, 
        Guid? relatedEntityId = null)
    {
        var notification = new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            RelatedEntityName = relatedEntityName,
            RelatedEntityId = relatedEntityId,
            IsRead = false
        };

        await _repository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();

        return notification;
    }

    public async Task<PagedResultDto<NotificationDto>> GetForUserAsync(Guid userId, NotificationQueryDto query)
    {
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);

        var queryable = _unitOfWork.Context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId);

        if (query.UnreadOnly == true)
        {
            queryable = queryable.Where(n => !n.IsRead);
        }

        if (!string.IsNullOrWhiteSpace(query.Type))
        {
            queryable = queryable.Where(n => n.Type == query.Type);
        }

        var totalCount = await queryable.CountAsync();

        var items = await queryable
            .OrderByDescending(n => n.CreatedAt)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new PagedResultDto<NotificationDto>
        {
            Items = items.Select(n => _mapper.Map<NotificationDto>(n)).ToList(),
            TotalCount = totalCount,
            PageSize = query.PageSize
        };
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        return await _unitOfWork.Context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsRead)
            .CountAsync();
    }

    public async Task MarkReadAsync(Guid userId, Guid notificationId)
    {
        var notification = await _repository.GetByIdAsync(notificationId);
        
        // We throw NotFoundException if it doesn't exist OR if it doesn't belong to the user,
        // specifically to avoid leaking information via a 403 Forbidden about other users' notifications.
        if (notification == null || notification.UserId != userId)
        {
            throw new NotFoundException($"Notification with ID {notificationId} was not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            
            _repository.Update(notification);
            await _unitOfWork.SaveChangesAsync();
        }
    }

    public async Task MarkAllReadAsync(Guid userId)
    {
        var unreadNotifications = await _unitOfWork.Context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        if (unreadNotifications.Any())
        {
            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.UtcNow;
                _repository.Update(notification);
            }
            
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
