using System;

namespace FleetMind.Api.DTOs.Cargo
{
    public class CreateCargoDto
    {
        public Guid VoyageId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public decimal DeclaredValue { get; set; }
        public string ConsigneeName { get; set; } = string.Empty;
        public string? HazardNotes { get; set; }
    }
}
