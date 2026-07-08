using System;

namespace FleetMind.Api.DTOs.Documents;

public class CreateDocumentDto
{
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
}
