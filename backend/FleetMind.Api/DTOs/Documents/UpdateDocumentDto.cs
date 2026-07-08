namespace FleetMind.Api.DTOs.Documents;

public class UpdateDocumentDto
{
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
}
