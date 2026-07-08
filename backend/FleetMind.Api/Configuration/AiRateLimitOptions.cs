namespace FleetMind.Api.Configuration;

public class AiRateLimitOptions
{
    public const string SectionName = "AiRateLimit";
    public int MaxRequestsPerUserPerHour { get; set; } = 20;
}
