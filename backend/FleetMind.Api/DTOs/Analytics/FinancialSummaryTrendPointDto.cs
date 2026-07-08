namespace FleetMind.Api.DTOs.Analytics
{
    public class FinancialSummaryTrendPointDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal FuelCost { get; set; }
        public decimal MaintenanceCost { get; set; }
        public decimal TotalOperatingCost { get; set; }
    }
}
