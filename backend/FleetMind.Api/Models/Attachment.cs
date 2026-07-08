using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models
{
    public class Attachment : BaseEntity
    {
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public Guid UploadedByUserId { get; set; }
    }
}
