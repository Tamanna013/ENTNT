using System;
using FleetMind.Api.Common;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class AuditLog : BaseEntity
    {
        public Guid? UserId { get; set; }
        public string UserName { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public string EntityId { get; set; } = null!;
        public string? Changes { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
