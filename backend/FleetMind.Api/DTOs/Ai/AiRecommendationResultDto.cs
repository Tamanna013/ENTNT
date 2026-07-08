using System;
using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Ai
{
    public class AiRecommendationResultDto
    {
        public List<string> Recommendations { get; set; } = new List<string>();
        public bool IsAvailable { get; set; }
        public DateTime GeneratedAt { get; set; }
        public string Disclaimer { get; set; } = string.Empty;
    }
}
