using System;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class DocumentVersion : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Document Document { get; set; } = null!;
    
    public int VersionNumber { get; set; }
    
    public Guid AttachmentId { get; set; }
    public Attachment Attachment { get; set; } = null!;
    
    public Guid UploadedByUserId { get; set; }
    public User UploadedByUser { get; set; } = null!;
    
    public string? ChangeNotes { get; set; }
}
