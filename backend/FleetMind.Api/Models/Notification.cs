using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class Notification : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Type { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Message { get; set; } = null!;
    
    public string? RelatedEntityName { get; set; }
    public Guid? RelatedEntityId { get; set; }
    
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
