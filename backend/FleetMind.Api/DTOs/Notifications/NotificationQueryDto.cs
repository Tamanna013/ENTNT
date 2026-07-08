using FleetMind.Api.DTOs.Common;

namespace FleetMind.Api.DTOs.Notifications;

public class NotificationQueryDto : PaginationQueryDto
{
    public bool? UnreadOnly { get; set; }
    public string? Type { get; set; }
}
