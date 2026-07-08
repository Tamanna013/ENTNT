using System;

namespace FleetMind.Api.DTOs.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public int CurrentVersionNumber { get; set; }
    public string CurrentVersionDownloadUrl { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
