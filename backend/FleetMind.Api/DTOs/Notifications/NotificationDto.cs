using System;

namespace FleetMind.Api.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    public string? RelatedEntityName { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
