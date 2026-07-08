using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Services;

public class AiConversationService : IAiConversationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public AiConversationService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<AiConversationDto> CreateConversationAsync()
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        if (userId == Guid.Empty) throw new UnauthorizedAccessException();

        var repo = _unitOfWork.Repository<AiConversation>();
        var conversation = new AiConversation
        {
            UserId = userId,
            Title = "New Conversation",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await repo.AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(conversation);
    }

    public async Task<AiConversationDto> GetConversationAsync(Guid id)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var repo = _unitOfWork.Repository<AiConversation>();
        
        // Use generic repo FindAsync but it doesn't include Messages.
        // We will just do a manual query via the context for the full aggregate
        // Wait, IGenericRepository doesn't expose context, so we'll just inject the context or we can cast
        // Let's assume generic repo has GetAllAsync() which we can't do .Include() on.
        // Instead, let's manually fetch the messages.
        
        var conversations = await repo.FindAsync(x => x.Id == id && x.UserId == userId);
        var conversation = conversations.FirstOrDefault();
        
        if (conversation == null)
            throw new NotFoundException($"Conversation with ID {id} not found.");

        var msgRepo = _unitOfWork.Repository<AiChatMessage>();
        var messages = await msgRepo.FindAsync(x => x.ConversationId == id);
        conversation.Messages = messages.OrderBy(m => m.CreatedAt).ToList();

        return MapToDto(conversation);
    }

    public async Task<List<AiConversationDto>> GetConversationsAsync()
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var repo = _unitOfWork.Repository<AiConversation>();
        
        var conversations = await repo.FindAsync(x => x.UserId == userId);
        
        return conversations
            .OrderByDescending(x => x.UpdatedAt)
            .Select(c => new AiConversationDto
            {
                Id = c.Id,
                Title = c.Title,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt ?? c.CreatedAt
            }).ToList();
    }

    public async Task<AiConversationDto> UpdateConversationAsync(Guid id, UpdateConversationDto dto)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var repo = _unitOfWork.Repository<AiConversation>();
        
        var conversations = await repo.FindAsync(x => x.Id == id && x.UserId == userId);
        var conversation = conversations.FirstOrDefault();
        
        if (conversation == null)
            throw new NotFoundException($"Conversation with ID {id} not found.");

        conversation.Title = dto.Title;
        conversation.UpdatedAt = DateTime.UtcNow;

        repo.Update(conversation);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(conversation);
    }

    public async Task DeleteConversationAsync(Guid id)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var repo = _unitOfWork.Repository<AiConversation>();
        
        var conversations = await repo.FindAsync(x => x.Id == id && x.UserId == userId);
        var conversation = conversations.FirstOrDefault();
        
        if (conversation == null)
            throw new NotFoundException($"Conversation with ID {id} not found.");

        repo.Remove(conversation);
        await _unitOfWork.SaveChangesAsync();
    }

    private static AiConversationDto MapToDto(AiConversation c)
    {
        return new AiConversationDto
        {
            Id = c.Id,
            Title = c.Title,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt ?? c.CreatedAt,
            Messages = c.Messages?.Select(m => new AiChatMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            }).ToList() ?? new List<AiChatMessageDto>()
        };
    }
}
