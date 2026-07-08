using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class CrewCertification : BaseEntity
    {
        public Guid CrewMemberId { get; set; }
        public CrewMember CrewMember { get; set; } = null!;

        public Guid AttachmentId { get; set; }
        public Attachment Attachment { get; set; } = null!;

        public string CertificationName { get; set; } = string.Empty;
        public DateOnly ExpiryDate { get; set; }
    }
}
