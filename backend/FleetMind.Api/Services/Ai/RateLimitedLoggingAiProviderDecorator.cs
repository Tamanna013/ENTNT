using System.Runtime.CompilerServices;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Configuration;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using FleetMind.Api.Common;

namespace FleetMind.Api.Services.Ai;

public class RateLimitedLoggingAiProviderDecorator : IAiProvider
{
    private readonly IAiProvider _innerProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AiRateLimitOptions _options;

    public RateLimitedLoggingAiProviderDecorator(
        IAiProvider innerProvider,
        IServiceScopeFactory scopeFactory,
        IOptions<AiRateLimitOptions> options)
    {
        _innerProvider = innerProvider;
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public bool IsAvailable => _innerProvider.IsAvailable;

    public async Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request)
    {
        using var scope = _scopeFactory.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User context is required for AI operations.");
        }

        await CheckRateLimitAsync(userId.Value, unitOfWork);

        AiUsageLog log = new()
        {
            UserId = userId.Value,
            Timestamp = DateTime.UtcNow,
            FeatureContext = null // Could potentially be parsed from request context if needed
        };

        try
        {
            var result = await _innerProvider.CompleteAsync(request);
            log.WasSuccessful = result.IsSuccess;
            log.TokensUsed = result.TokensUsed;
            return result;
        }
        catch
        {
            log.WasSuccessful = false;
            throw;
        }
        finally
        {
            var repo = unitOfWork.Repository<AiUsageLog>();
            await repo.AddAsync(log);
            await unitOfWork.SaveChangesAsync();
        }
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AiCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            throw new UnauthorizedAccessException("User context is required for AI operations.");
        }

        await CheckRateLimitAsync(userId.Value, unitOfWork);

        var stream = _innerProvider.StreamCompleteAsync(request, cancellationToken);
        
        await foreach (var chunk in stream)
        {
            yield return chunk;
        }

        // Only log after the stream fully completes successfully.
        // If it throws during streaming, the calling middleware handles the error, 
        // but for simplicity we log stream successes here once finished.
        var repo = unitOfWork.Repository<AiUsageLog>();
        await repo.AddAsync(new AiUsageLog
        {
            UserId = userId.Value,
            Timestamp = DateTime.UtcNow,
            WasSuccessful = true,
            FeatureContext = null
        });
        await unitOfWork.SaveChangesAsync();
    }

    private async Task CheckRateLimitAsync(Guid userId, IUnitOfWork unitOfWork)
    {
        var oneHourAgo = DateTime.UtcNow.AddHours(-1);
        
        var repo = unitOfWork.Repository<AiUsageLog>();
        var count = await repo.CountAsync(x => x.UserId == userId && x.Timestamp >= oneHourAgo);

        if (count >= _options.MaxRequestsPerUserPerHour)
        {
            throw new RateLimitExceededException("You have exceeded the AI usage limit for this hour. Please try again later.");
        }
    }
}
