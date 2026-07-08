using System;

namespace FleetMind.Api.DTOs.Ai
{
    public class AiSummaryResultDto
    {
        public string Summary { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
