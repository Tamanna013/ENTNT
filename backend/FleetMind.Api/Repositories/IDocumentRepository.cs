using System.Collections.Generic;
using System.Threading.Tasks;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Documents;

namespace FleetMind.Api.Repositories;

public interface IDocumentRepository : IGenericRepository<Document>
{
    Task<(List<Document> items, int totalCount)> GetPagedAsync(DocumentQueryDto query);
    
    Task AddVersionAsync(DocumentVersion version);
    Task<List<DocumentVersion>> GetVersionsAsync(Guid documentId);
    Task<DocumentVersion?> GetVersionAsync(Guid documentId, int versionNumber);
}
