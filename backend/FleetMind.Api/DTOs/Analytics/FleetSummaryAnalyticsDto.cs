using System;

namespace FleetMind.Api.DTOs.Analytics
{
    public class FleetSummaryAnalyticsDto
    {
        public int TotalFleets { get; set; }
        public int TotalShips { get; set; }
        public int ActiveShips { get; set; }
        public int TotalCrew { get; set; }
        public int AssignedCrew { get; set; }
        public DateTime GeneratedAt { get; set; }
    }
}
