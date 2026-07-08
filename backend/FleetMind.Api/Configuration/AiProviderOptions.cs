namespace FleetMind.Api.Configuration;

public class AiProviderOptions
{
    public const string SectionName = "AiProvider";
    
    // Expected values: "AzureOpenAI", "Gemini", or "None"
    public string Provider { get; set; } = "None";
    
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    
    public string ModelDeploymentName { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 30;
}
