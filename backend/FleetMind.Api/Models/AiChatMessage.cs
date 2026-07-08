using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class AiChatMessage : BaseEntity
{
    public Guid ConversationId { get; set; }
    public AiConversation Conversation { get; set; } = null!;
    
    public string Role { get; set; } = string.Empty; // "User", "Assistant", "System"
    public string Content { get; set; } = string.Empty;
}
