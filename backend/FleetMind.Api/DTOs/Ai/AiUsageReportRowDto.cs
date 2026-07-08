namespace FleetMind.Api.DTOs.Ai;

public class AiUsageReportRowDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int RequestCount { get; set; }
    public int SuccessCount { get; set; }
    public int? TotalTokensUsed { get; set; }
}
