using System;

namespace FleetMind.Api.DTOs.Fuel
{
    public class FuelLogDto
    {
        public Guid Id { get; set; }
        public Guid ShipId { get; set; }
        public string ShipName { get; set; } = string.Empty;
        
        public Guid? VoyageId { get; set; }
        public string? VoyageNumber { get; set; }
        
        public string FuelType { get; set; } = string.Empty;
        public decimal QuantityLiters { get; set; }
        public decimal CostPerLiter { get; set; }
        public decimal TotalCost { get; set; }
        
        public DateTime RecordedDate { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
