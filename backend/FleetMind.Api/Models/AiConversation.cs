using System.Collections.Generic;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class AiConversation : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    
    public List<AiChatMessage> Messages { get; set; } = new();
}
