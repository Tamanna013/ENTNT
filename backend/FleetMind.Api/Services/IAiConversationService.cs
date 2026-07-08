using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services;

public interface IAiConversationService
{
    Task<AiConversationDto> CreateConversationAsync();
    Task<AiConversationDto> GetConversationAsync(Guid id);
    Task<List<AiConversationDto>> GetConversationsAsync();
    Task<AiConversationDto> UpdateConversationAsync(Guid id, UpdateConversationDto dto);
    Task DeleteConversationAsync(Guid id);
}
