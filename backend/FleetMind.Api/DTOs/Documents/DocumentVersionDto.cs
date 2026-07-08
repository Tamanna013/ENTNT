using System;

namespace FleetMind.Api.DTOs.Documents;

public class DocumentVersionDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string DownloadUrl { get; set; } = null!;
    public string FileName { get; set; } = null!;
    public Guid UploadedByUserId { get; set; }
    public string UploadedByUserName { get; set; } = null!;
    public string? ChangeNotes { get; set; }
    public DateTime CreatedAt { get; set; }
}
