using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Common;

namespace FleetMind.Api.Services.Ai;

public interface IAiProvider
{
    bool IsAvailable { get; }
    
    Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request);
    
    IAsyncEnumerable<string> StreamCompleteAsync(
        AiCompletionRequest request, 
        CancellationToken cancellationToken = default);
}
