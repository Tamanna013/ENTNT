using System;

namespace FleetMind.Api.DTOs.Fuel
{
    public class CreateFuelLogDto
    {
        public Guid ShipId { get; set; }
        public Guid? VoyageId { get; set; }
        
        public string FuelType { get; set; } = string.Empty;
        public decimal QuantityLiters { get; set; }
        public decimal CostPerLiter { get; set; }
        
        public DateTime RecordedDate { get; set; }
        public string? Notes { get; set; }
    }
}
