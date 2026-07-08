using FleetMind.Api.Configuration;
using FleetMind.Api.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Asp.Versioning;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Services;
using Microsoft.EntityFrameworkCore;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/ai")]
[Authorize]
public class AiController : ControllerBase
{
    private readonly IAiProvider _aiProvider;
    private readonly AiProviderOptions _options;
    private readonly INaturalLanguageSearchService _nlSearchService;

    public AiController(IAiProvider aiProvider, IOptions<AiProviderOptions> options, INaturalLanguageSearchService nlSearchService)
    {
        _aiProvider = aiProvider;
        _options = options.Value;
        _nlSearchService = nlSearchService;
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new 
        {
            provider = _options.Provider,
            isAvailable = _aiProvider.IsAvailable
        });
    }

    [HttpPost("natural-language-search")]
    public async Task<ActionResult<NaturalLanguageSearchResultDto>> NaturalLanguageSearch([FromBody] NaturalLanguageSearchRequestDto request)
    {
        var result = await _nlSearchService.ParseAndSearchAsync(request.Query);
        return Ok(result);
    }

    [HttpGet("conversations")]
    public async Task<ActionResult<List<AiConversationDto>>> GetConversations([FromServices] IAiConversationService conversationService)
    {
        var result = await conversationService.GetConversationsAsync();
        return Ok(result);
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<AiConversationDto>> CreateConversation([FromServices] IAiConversationService conversationService)
    {
        var result = await conversationService.CreateConversationAsync();
        return Ok(result);
    }

    [HttpGet("conversations/{id}")]
    public async Task<ActionResult<AiConversationDto>> GetConversation(Guid id, [FromServices] IAiConversationService conversationService)
    {
        var result = await conversationService.GetConversationAsync(id);
        return Ok(result);
    }

    [HttpPut("conversations/{id}")]
    public async Task<ActionResult<AiConversationDto>> UpdateConversation(Guid id, [FromBody] UpdateConversationDto dto, [FromServices] IAiConversationService conversationService)
    {
        var result = await conversationService.UpdateConversationAsync(id, dto);
        return Ok(result);
    }

    [HttpDelete("conversations/{id}")]
    public async Task<IActionResult> DeleteConversation(Guid id, [FromServices] IAiConversationService conversationService)
    {
        await conversationService.DeleteConversationAsync(id);
        return NoContent();
    }

    [HttpPost("conversations/{id}/messages")]
    public async Task StreamMessage(
        Guid id, 
        [FromBody] SendMessageDto request,
        [FromServices] IAiConversationService conversationService,
        [FromServices] IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        // 1. Validate conversation exists and owned by user
        var conversation = await conversationService.GetConversationAsync(id);
        
        // 2. Save user message
        var msgRepo = unitOfWork.Repository<Models.AiChatMessage>();
        var userMsg = new Models.AiChatMessage
        {
            ConversationId = id,
            Role = "User",
            Content = request.Message
        };
        await msgRepo.AddAsync(userMsg);
        await unitOfWork.SaveChangesAsync();

        // 3. Set up SSE response
        Response.Headers.Add("Content-Type", "text/event-stream");
        Response.Headers.Add("Cache-Control", "no-cache");
        Response.Headers.Add("Connection", "keep-alive");
        
        if (!_aiProvider.IsAvailable)
        {
            var errJson = System.Text.Json.JsonSerializer.Serialize(new { error = "AI Provider is not configured or available." });
            await Response.WriteAsync($"data: {errJson}\n\n");
            await Response.Body.FlushAsync();
            return;
        }

        // 4. Build prompt (incorporate previous messages)
        var contextStr = string.Join("\n", conversation.Messages.Select(m => $"{m.Role}: {m.Content}"));
        
        var aiRequest = new FleetMind.Api.Common.AiCompletionRequest
        {
            SystemPrompt = "You are FleetMind AI, a helpful shipping and logistics assistant.",
            UserPrompt = $"Chat History:\n{contextStr}\n\nUser: {request.Message}"
        };

        var fullResponse = "";
        
        try 
        {
            await foreach (var chunk in _aiProvider.StreamCompleteAsync(aiRequest, cancellationToken))
            {
                fullResponse += chunk;
                var safeChunk = chunk.Replace("\n", "\\n").Replace("\r", "");
                var chunkJson = System.Text.Json.JsonSerializer.Serialize(new { content = safeChunk });
                await Response.WriteAsync($"data: {chunkJson}\n\n");
                await Response.Body.FlushAsync();
            }
            
            // Save AI message
            var aiMsg = new Models.AiChatMessage
            {
                ConversationId = id,
                Role = "Assistant",
                Content = fullResponse
            };
            await msgRepo.AddAsync(aiMsg);
            
            // Update conversation UpdatedAt
            var convRepo = unitOfWork.Repository<Models.AiConversation>();
            var convEntity = await convRepo.GetByIdAsync(id);
            if (convEntity != null) {
                convEntity.UpdatedAt = DateTime.UtcNow;
                convRepo.Update(convEntity);
            }
            
            await unitOfWork.SaveChangesAsync();
        }
        catch (FleetMind.Api.Common.Exceptions.RateLimitExceededException)
        {
            // Specifically handle 429
            Response.StatusCode = 429;
            var errJson = System.Text.Json.JsonSerializer.Serialize(new { error = "You've reached the AI usage limit for this hour. Please try again later." });
            await Response.WriteAsync($"data: {errJson}\n\n");
            await Response.Body.FlushAsync();
        }
        catch (Exception ex)
        {
            var errJson = System.Text.Json.JsonSerializer.Serialize(new { error = ex.Message });
            await Response.WriteAsync($"data: {errJson}\n\n");
            await Response.Body.FlushAsync();
        }
    }

    [HttpGet("usage-report")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<AiUsageReportRowDto>>> GetUsageReport(
        [FromServices] FleetMind.Api.Data.FleetMindDbContext dbContext,
        [FromQuery] DateTime? dateFrom, 
        [FromQuery] DateTime? dateTo)
    {
        var query = dbContext.AiUsageLogs.AsQueryable();

        if (dateFrom.HasValue)
        {
            query = query.Where(x => x.Timestamp >= dateFrom.Value);
        }
        
        if (dateTo.HasValue)
        {
            query = query.Where(x => x.Timestamp <= dateTo.Value);
        }

        var results = await query
            .Include(x => x.User)
            .GroupBy(x => new { x.UserId, x.User.FirstName, x.User.LastName })
            .Select(g => new AiUsageReportRowDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.FirstName + " " + g.Key.LastName,
                RequestCount = g.Count(),
                SuccessCount = g.Count(x => x.WasSuccessful),
                TotalTokensUsed = g.Sum(x => x.TokensUsed ?? 0)
            })
            .OrderByDescending(x => x.RequestCount)
            .ToListAsync();

        return Ok(results);
    }
}
