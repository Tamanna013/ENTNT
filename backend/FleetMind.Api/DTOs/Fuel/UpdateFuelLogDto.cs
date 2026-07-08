using System;

namespace FleetMind.Api.DTOs.Fuel
{
    public class UpdateFuelLogDto
    {
        public decimal QuantityLiters { get; set; }
        public decimal CostPerLiter { get; set; }
        public DateTime RecordedDate { get; set; }
        public string? Notes { get; set; }
    }
}
