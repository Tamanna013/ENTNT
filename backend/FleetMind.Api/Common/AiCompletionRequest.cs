namespace FleetMind.Api.Common;

public class AiCompletionRequest
{
    public string SystemPrompt { get; set; } = string.Empty;
    public string UserPrompt { get; set; } = string.Empty;
    public int? MaxTokensOverride { get; set; }
}
