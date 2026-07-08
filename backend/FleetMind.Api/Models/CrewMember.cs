using System;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class CrewMember : BaseEntity
    {
        public Guid? ShipId { get; set; }
        public Ship? Ship { get; set; }

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Rank { get; set; } = string.Empty;
        public string Status { get; set; } = CrewStatus.Unassigned;
        public string Nationality { get; set; } = string.Empty;
        
        public DateOnly DateOfBirth { get; set; }
        public string LicenseNumber { get; set; } = string.Empty;
        public DateOnly HireDate { get; set; }
        
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        
        public ICollection<CrewCertification> Certifications { get; set; } = new List<CrewCertification>();
    }
}
