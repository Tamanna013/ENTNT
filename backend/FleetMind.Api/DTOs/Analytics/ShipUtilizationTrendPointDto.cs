namespace FleetMind.Api.DTOs.Analytics
{
    public class ShipUtilizationTrendPointDto
    {
        public string Month { get; set; } = string.Empty;
        public int TotalShips { get; set; }
        public int ActiveShips { get; set; }
        public decimal UtilizationPercentage { get; set; }
    }
}
