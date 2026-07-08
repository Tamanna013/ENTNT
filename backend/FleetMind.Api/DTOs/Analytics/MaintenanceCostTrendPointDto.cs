namespace FleetMind.Api.DTOs.Analytics
{
    public class MaintenanceCostTrendPointDto
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalEstimatedCost { get; set; }
        public decimal TotalActualCost { get; set; }
        public decimal VariancePercentage { get; set; }
    }
}
