using System;

namespace FleetMind.Api.DTOs.Crew
{
    public class CrewCertificationDto
    {
        public Guid Id { get; set; }
        public Guid CrewMemberId { get; set; }
        public string CertificationName { get; set; } = string.Empty;
        public DateOnly ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
