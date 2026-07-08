using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using FleetMind.Api.Common;
using FleetMind.Api.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace FleetMind.Api.Services.Ai;

public class AzureOpenAiProvider : IAiProvider
{
    private readonly AiProviderOptions _options;
    private readonly ILogger<AzureOpenAiProvider> _logger;
    private readonly AzureOpenAIClient? _client;

    public AzureOpenAiProvider(IOptions<AiProviderOptions> options, ILogger<AzureOpenAiProvider> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (IsConfigured(_options))
        {
            try
            {
                _client = new AzureOpenAIClient(
                    new Uri(_options.Endpoint!),
                    new AzureKeyCredential(_options.ApiKey!));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to instantiate AzureOpenAIClient. AI will be reported unavailable.");
            }
        }
    }

    private static bool IsConfigured(AiProviderOptions opts)
    {
        return !string.IsNullOrWhiteSpace(opts.ApiKey) && !string.IsNullOrWhiteSpace(opts.Endpoint);
    }

    public bool IsAvailable => _client != null;

    public async Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request)
    {
        if (!IsAvailable)
        {
            return new AiCompletionResult { IsSuccess = false, ErrorMessage = "Azure OpenAI is not properly configured." };
        }

        try
        {
            var chatClient = _client!.GetChatClient(_options.ModelDeploymentName);
            
            var messages = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new SystemChatMessage(request.SystemPrompt));
            }
            messages.Add(new UserChatMessage(request.UserPrompt));

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = request.MaxTokensOverride ?? _options.MaxTokens
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            var response = await chatClient.CompleteChatAsync(messages, options, cts.Token);
            var content = response.Value.Content[0].Text;

            return new AiCompletionResult
            {
                IsSuccess = true,
                Content = content,
                TokensUsed = response.Value.Usage?.TotalTokenCount
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Azure OpenAI completion timed out.");
            return new AiCompletionResult { IsSuccess = false, ErrorMessage = "AI request timed out." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure OpenAI completion failed.");
            return new AiCompletionResult { IsSuccess = false, ErrorMessage = "An error occurred communicating with the AI provider." };
        }
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AiCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            yield return "Azure OpenAI is not properly configured.";
            yield break;
        }

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

        IAsyncEnumerable<StreamingChatCompletionUpdate>? responseStream = null;
        string? errorMessage = null;

        try
        {
            var chatClient = _client!.GetChatClient(_options.ModelDeploymentName);
            
            var messages = new List<ChatMessage>();
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                messages.Add(new SystemChatMessage(request.SystemPrompt));
            }
            messages.Add(new UserChatMessage(request.UserPrompt));

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = request.MaxTokensOverride ?? _options.MaxTokens
            };

            responseStream = chatClient.CompleteChatStreamingAsync(messages, options, cts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Azure OpenAI streaming completion.");
            errorMessage = "An error occurred starting the AI stream.";
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
            yield break;
        }

        if (responseStream != null)
        {
            await foreach (var update in responseStream.WithCancellation(cts.Token))
            {
                if (update.ContentUpdate.Count > 0)
                {
                    yield return update.ContentUpdate[0].Text;
                }
            }
        }
    }
}
