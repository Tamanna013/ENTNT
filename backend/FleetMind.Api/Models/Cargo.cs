using System;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Cargo : BaseEntity
    {
        public Guid VoyageId { get; set; }
        public Voyage Voyage { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = CargoStatus.Pending;
        
        public decimal WeightKg { get; set; }
        public decimal DeclaredValue { get; set; }
        
        public string ConsigneeName { get; set; } = string.Empty;
        public string? HazardNotes { get; set; }
    }
}
