using System;

namespace FleetMind.Api.DTOs.Crew
{
    public class UploadCertificationDto
    {
        public string CertificationName { get; set; } = string.Empty;
        public DateOnly ExpiryDate { get; set; }
    }
}
