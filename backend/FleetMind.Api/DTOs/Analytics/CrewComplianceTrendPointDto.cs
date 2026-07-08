namespace FleetMind.Api.DTOs.Analytics
{
    public class CrewComplianceTrendPointDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalActiveCertifications { get; set; }
        public int ExpiredCount { get; set; }
        public decimal ComplianceRate { get; set; }
    }
}
