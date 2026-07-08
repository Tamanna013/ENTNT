using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Common;

namespace FleetMind.Api.Services.Ai;

public class NullAiProvider : IAiProvider
{
    private const string NotConfiguredMessage = "AI features are not currently configured for this environment.";

    public bool IsAvailable => false;

    public Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request)
    {
        return Task.FromResult(new AiCompletionResult
        {
            IsSuccess = false,
            ErrorMessage = NotConfiguredMessage,
            Content = string.Empty
        });
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AiCompletionRequest request, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return NotConfiguredMessage;
        await Task.CompletedTask;
    }
}
