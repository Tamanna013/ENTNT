using System;
using Microsoft.AspNetCore.Http;

namespace FleetMind.Api.DTOs.Attachments
{
    public class UploadAttachmentRequestDto
    {
        public string EntityName { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
