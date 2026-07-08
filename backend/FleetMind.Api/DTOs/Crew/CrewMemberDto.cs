using System;

namespace FleetMind.Api.DTOs.Crew
{
    public class CrewMemberDto
    {
        public Guid Id { get; set; }
        public Guid? ShipId { get; set; }
        public string? ShipName { get; set; }
        
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        
        public DateOnly DateOfBirth { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateOnly HireDate { get; set; }
        
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
