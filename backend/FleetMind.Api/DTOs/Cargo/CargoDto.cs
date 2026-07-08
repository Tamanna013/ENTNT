using System;
using System.Collections.Generic;

namespace FleetMind.Api.DTOs.Cargo
{
    public class CargoDto
    {
        public Guid Id { get; set; }
        public Guid VoyageId { get; set; }
        public string VoyageNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public decimal DeclaredValue { get; set; }
        public string ConsigneeName { get; set; } = string.Empty;
        public string? HazardNotes { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // This is informational metadata populated at creation time, not persisted
        public List<string> Warnings { get; set; } = new List<string>();
    }
}
