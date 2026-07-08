using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Documents;
using Microsoft.AspNetCore.Http;

namespace FleetMind.Api.Services;

public interface IDocumentService
{
    Task<PagedResultDto<DocumentDto>> GetDocumentsAsync(DocumentQueryDto query);
    Task<DocumentDto> GetDocumentByIdAsync(Guid id);
    Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto dto, IFormFile file, Guid uploadedByUserId);
    Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto dto);
    Task DeleteDocumentAsync(Guid id);
    
    Task<DocumentVersionDto> UploadNewVersionAsync(Guid documentId, IFormFile file, string? changeNotes, Guid uploadedByUserId);
    Task<List<DocumentVersionDto>> GetVersionsAsync(Guid documentId);
    Task<(Stream stream, string contentType, string fileName)> DownloadVersionAsync(Guid documentId, int versionNumber);
}
