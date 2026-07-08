using System;

namespace FleetMind.Api.DTOs.Crew
{
    public class UpdateCrewMemberDto
    {
        // DateOfBirth, LicenseNumber, and HireDate are deliberately excluded for this milestone's scope
        // (treated as fixed identity/historical fields). Standard updates shouldn't touch them.
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
    }
}
