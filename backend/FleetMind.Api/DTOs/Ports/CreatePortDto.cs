namespace FleetMind.Api.DTOs.Ports
{
    public class CreatePortDto
    {
        public string Name { get; set; } = string.Empty;
        public string UnLocode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
}
