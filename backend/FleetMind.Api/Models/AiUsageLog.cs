using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class AiUsageLog : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string? FeatureContext { get; set; }
    public int? TokensUsed { get; set; }
    public bool WasSuccessful { get; set; }
    public DateTime Timestamp { get; set; }
}
