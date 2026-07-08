using System;

namespace FleetMind.Api.DTOs.Crew
{
    public class CreateCrewMemberDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        
        public DateOnly DateOfBirth { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateOnly HireDate { get; set; }
        
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}
