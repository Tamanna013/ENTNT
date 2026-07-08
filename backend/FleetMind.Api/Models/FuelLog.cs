using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class FuelLog : BaseEntity
    {
        public Guid ShipId { get; set; }
        public Ship Ship { get; set; } = null!;

        public Guid? VoyageId { get; set; }
        public Voyage? Voyage { get; set; }

        public string FuelType { get; set; } = null!;
        public decimal QuantityLiters { get; set; }
        public decimal CostPerLiter { get; set; }
        public DateTime RecordedDate { get; set; }
        public string? Notes { get; set; }
    }
}
