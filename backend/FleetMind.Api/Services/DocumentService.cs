using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Documents;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly IAttachmentService _attachmentService;

    public DocumentService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        IAttachmentService attachmentService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _attachmentService = attachmentService;
    }

    public async Task<PagedResultDto<DocumentDto>> GetDocumentsAsync(DocumentQueryDto query)
    {
        var (items, totalCount) = await _unitOfWork.Documents.GetPagedAsync(query);

        return new PagedResultDto<DocumentDto>
        {
            Items = _mapper.Map<List<DocumentDto>>(items),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<DocumentDto> GetDocumentByIdAsync(Guid id)
    {
        var query = new DocumentQueryDto { PageNumber = 1, PageSize = 1 };
        // We use the repository's query with includes instead of GetByIdAsync
        var (items, _) = await _unitOfWork.Documents.GetPagedAsync(query);
        var document = items.FirstOrDefault(d => d.Id == id);

        if (document == null)
            throw new NotFoundException("Document", id);

        return _mapper.Map<DocumentDto>(document);
    }

    public async Task<DocumentDto> CreateDocumentAsync(CreateDocumentDto dto, IFormFile file, Guid uploadedByUserId)
    {
        var document = new Document
        {
            Title = dto.Title,
            Category = dto.Category,
            Description = dto.Description,
            EntityName = dto.EntityName,
            EntityId = dto.EntityId,
            CurrentVersionNumber = 1
        };

        await _unitOfWork.Documents.AddAsync(document);
        // Save first to get the Document ID for the attachment
        await _unitOfWork.SaveChangesAsync();

        // Upload the file using the existing attachment service
        var attachmentDto = await _attachmentService.UploadAsync(
            file, 
            AttachmentEntityType.DocumentVersion, 
            document.Id, 
            uploadedByUserId);

        var documentVersion = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = 1,
            AttachmentId = attachmentDto.Id,
            UploadedByUserId = uploadedByUserId,
            ChangeNotes = "Initial version"
        };

        // We can just add it via generic repository, but it's easier since Document has Versions collection
        document.Versions.Add(documentVersion);
        await _unitOfWork.SaveChangesAsync();

        // We need to fetch it again with all includes to map correctly
        return await GetDocumentByIdAsync(document.Id);
    }

    public async Task<DocumentDto> UpdateDocumentAsync(Guid id, UpdateDocumentDto dto)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
            throw new NotFoundException("Document", id);

        document.Title = dto.Title;
        document.Category = dto.Category;
        document.Description = dto.Description;

        _unitOfWork.Documents.Update(document);
        await _unitOfWork.SaveChangesAsync();

        return await GetDocumentByIdAsync(id);
    }

    public async Task DeleteDocumentAsync(Guid id)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(id);
        if (document == null)
            throw new NotFoundException("Document", id);

        // Soft delete the document
        // We INTENTIONALLY do not cascade hard-delete the underlying DocumentVersions and Attachments.
        // A document's version history may retain compliance/regulatory value even after the logical
        // document record is retired, so this is an intentional divergence from the certification pattern.
        _unitOfWork.Documents.Remove(document);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DocumentVersionDto> UploadNewVersionAsync(Guid documentId, IFormFile file, string? changeNotes, Guid uploadedByUserId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
            throw new NotFoundException("Document", documentId);

        // Compute new version based on denormalized counter to avoid gaps from potential soft-deleted versions
        var newVersionNumber = document.CurrentVersionNumber + 1;

        var attachmentDto = await _attachmentService.UploadAsync(
            file, 
            AttachmentEntityType.DocumentVersion, 
            document.Id, 
            uploadedByUserId);

        var documentVersion = new DocumentVersion
        {
            DocumentId = document.Id,
            VersionNumber = newVersionNumber,
            AttachmentId = attachmentDto.Id,
            UploadedByUserId = uploadedByUserId,
            ChangeNotes = changeNotes
        };

        // Wait, how do I save documentVersion? Add to context
        await _unitOfWork.Documents.AddVersionAsync(documentVersion);
        
        document.CurrentVersionNumber = newVersionNumber;
        _unitOfWork.Documents.Update(document);
        
        await _unitOfWork.SaveChangesAsync();

        // Load the navigation properties for the DTO map
        var version = await _unitOfWork.Documents.GetVersionAsync(document.Id, newVersionNumber);

        return _mapper.Map<DocumentVersionDto>(version);
    }

    public async Task<List<DocumentVersionDto>> GetVersionsAsync(Guid documentId)
    {
        var document = await _unitOfWork.Documents.GetByIdAsync(documentId);
        if (document == null)
            throw new NotFoundException("Document", documentId);

        var versions = await _unitOfWork.Documents.GetVersionsAsync(documentId);

        return _mapper.Map<List<DocumentVersionDto>>(versions);
    }

    public async Task<(Stream stream, string contentType, string fileName)> DownloadVersionAsync(Guid documentId, int versionNumber)
    {
        var version = await _unitOfWork.Documents.GetVersionAsync(documentId, versionNumber);

        if (version == null)
            throw new NotFoundException($"DocumentVersion for Document {documentId} and Version {versionNumber}");

        return await _attachmentService.DownloadAsync(version.AttachmentId);
    }
}
