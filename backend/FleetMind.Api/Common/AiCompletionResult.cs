namespace FleetMind.Api.Common;

public class AiCompletionResult
{
    public string Content { get; set; } = string.Empty;
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public int? TokensUsed { get; set; }
}
