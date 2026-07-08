using System;
using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Ai;

public class AiConversationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public List<AiChatMessageDto> Messages { get; set; } = new();
}
