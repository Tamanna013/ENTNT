namespace FleetMind.Api.DTOs.Cargo
{
    public class UpdateCargoDto
    {
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal WeightKg { get; set; }
        public decimal DeclaredValue { get; set; }
        public string ConsigneeName { get; set; } = string.Empty;
        public string? HazardNotes { get; set; }
    }
}
