using System;
using System.Threading.Tasks;
using Asp.Versioning;
using FleetMind.Api.DTOs.Notifications;
using FleetMind.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FleetMind.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] // No specific role, every authenticated user manages their own notifications
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications([FromQuery] NotificationQueryDto query)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty) return Unauthorized();
        
        var result = await _notificationService.GetForUserAsync(userId.Value, query);
        return Ok(result);
    }

    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty) return Unauthorized();
        
        var count = await _notificationService.GetUnreadCountAsync(userId.Value);
        return Ok(new { count });
    }

    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkRead(Guid id)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty) return Unauthorized();
        
        await _notificationService.MarkReadAsync(userId.Value, id);
        return Ok();
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || userId.Value == Guid.Empty) return Unauthorized();
        
        await _notificationService.MarkAllReadAsync(userId.Value);
        return NoContent();
    }
    
}
