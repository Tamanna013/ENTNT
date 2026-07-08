using System;

namespace FleetMind.Api.DTOs.Reporting
{
    public class FuelEfficiencyReportRowDto
    {
        public Guid ShipId { get; set; }
        public string ShipName { get; set; } = string.Empty;
        public decimal TotalQuantityLiters { get; set; }
        public decimal TotalCost { get; set; }
        public decimal AverageCostPerLiter { get; set; }
        public int LogCount { get; set; }
    }
}
