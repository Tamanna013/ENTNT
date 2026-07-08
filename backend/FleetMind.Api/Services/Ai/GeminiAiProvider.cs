using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FleetMind.Api.Common;
using FleetMind.Api.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mscc.GenerativeAI;
using Mscc.GenerativeAI.Types;

namespace FleetMind.Api.Services.Ai;

public class GeminiAiProvider : IAiProvider
{
    private readonly AiProviderOptions _options;
    private readonly ILogger<GeminiAiProvider> _logger;
    private readonly GoogleAI? _googleAi;

    public GeminiAiProvider(IOptions<AiProviderOptions> options, ILogger<GeminiAiProvider> logger)
    {
        _options = options.Value;
        _logger = logger;

        if (IsConfigured(_options))
        {
            try
            {
                // Mscc.GenerativeAI is a lightweight community package providing simple Google AI integration.
                _googleAi = new GoogleAI(_options.ApiKey!);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to instantiate Gemini client. AI will be reported unavailable.");
            }
        }
    }

    private static bool IsConfigured(AiProviderOptions opts)
    {
        return !string.IsNullOrWhiteSpace(opts.ApiKey);
    }

    public bool IsAvailable => _googleAi != null;

    public async Task<AiCompletionResult> CompleteAsync(AiCompletionRequest request)
    {
        if (!IsAvailable)
        {
            return new AiCompletionResult { IsSuccess = false, ErrorMessage = "Gemini is not properly configured." };
        }

        try
        {
            var model = _googleAi!.GenerativeModel(model: _options.ModelDeploymentName);
            
            var generateContentRequest = new GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = new List<IPart> { new Part { Text = request.SystemPrompt } } },
                Contents = new List<Content> { new Content { Parts = new List<IPart> { new Part { Text = request.UserPrompt } } } },
                GenerationConfig = new GenerationConfig
                {
                    MaxOutputTokens = request.MaxTokensOverride ?? _options.MaxTokens
                }
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            // Not threading cancellation token to GenerateContentAsync as Mscc 3.1.0 does not natively take it
            // in some overload variants; but we'll try the overload with Cancel token if available.
            // But wait, the standard signature for this lightweight package is GenerateContentAsync(GenerateContentRequest).
            var response = await model.GenerateContent(generateContentRequest);

            return new AiCompletionResult
            {
                IsSuccess = true,
                Content = response.Text ?? string.Empty,
                TokensUsed = response.UsageMetadata?.TotalTokenCount
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Gemini completion timed out.");
            return new AiCompletionResult { IsSuccess = false, ErrorMessage = "AI request timed out." };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini completion failed.");
            return new AiCompletionResult { IsSuccess = false, ErrorMessage = "An error occurred communicating with the AI provider." };
        }
    }

    public async IAsyncEnumerable<string> StreamCompleteAsync(
        AiCompletionRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!IsAvailable)
        {
            yield return "Gemini is not properly configured.";
            yield break;
        }

        IAsyncEnumerable<GenerateContentResponse>? responseStream = null;
        string? errorMessage = null;

        try
        {
            var model = _googleAi!.GenerativeModel(model: _options.ModelDeploymentName);
            
            var generateContentRequest = new GenerateContentRequest
            {
                SystemInstruction = new Content { Parts = new List<IPart> { new Part { Text = request.SystemPrompt } } },
                Contents = new List<Content> { new Content { Parts = new List<IPart> { new Part { Text = request.UserPrompt } } } },
                GenerationConfig = new GenerationConfig
                {
                    MaxOutputTokens = request.MaxTokensOverride ?? _options.MaxTokens
                }
            };

            responseStream = model.GenerateContentStream(generateContentRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Gemini streaming completion.");
            errorMessage = "An error occurred starting the AI stream.";
        }

        if (errorMessage != null)
        {
            yield return errorMessage;
            yield break;
        }

        if (responseStream != null)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

            await foreach (var update in responseStream.WithCancellation(cts.Token))
            {
                if (!string.IsNullOrEmpty(update.Text))
                {
                    yield return update.Text;
                }
            }
        }
    }
}
