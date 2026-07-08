namespace FleetMind.Api.DTOs.Fleets
{
    public class UpdateFleetDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid HomePortId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
