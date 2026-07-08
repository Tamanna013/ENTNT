using System;
using System.Collections.Generic;
using FleetMind.Api.Models.Common;

namespace FleetMind.Api.Models;

public class Document : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Category { get; set; } = null!;
    public string? Description { get; set; }
    
    // Nullable optional polymorphic link
    public string? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    
    // Denormalized convenience field for cheap reads without always joining to DocumentVersions.
    // Keeping this in sync is the service layer's responsibility, not a schema-enforced guarantee.
    public int CurrentVersionNumber { get; set; } = 1;

    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
}
