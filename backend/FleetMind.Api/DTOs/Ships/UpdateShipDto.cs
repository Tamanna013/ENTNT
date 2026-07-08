namespace FleetMind.Api.DTOs.Ships
{
    public class UpdateShipDto
    {
        // Deliberately EXCLUDE FleetId (reassigning a ship to a different fleet is a more consequential operation, 
        // out of scope for this milestone's standard update).
        // EXCLUDE IMO (a real vessel's IMO number is a fixed identifier that shouldn't normally change post-creation).
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int YearBuilt { get; set; }
        public decimal GrossTonnage { get; set; }
        public string Flag { get; set; } = string.Empty;
    }
}
