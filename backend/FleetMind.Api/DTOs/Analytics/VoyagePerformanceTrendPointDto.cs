namespace FleetMind.Api.DTOs.Analytics
{
    public class VoyagePerformanceTrendPointDto
    {
        public string Month { get; set; } = string.Empty;
        public int CompletedVoyages { get; set; }
        public int OnTimeVoyages { get; set; }
        public decimal OnTimePercentage { get; set; }
    }
}
