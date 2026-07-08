using System;

namespace FleetMind.Api.DTOs.Attachments
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public Guid UploadedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
