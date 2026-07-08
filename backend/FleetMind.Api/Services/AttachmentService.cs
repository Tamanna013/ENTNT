using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Configuration;
using FleetMind.Api.DTOs.Attachments;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IFileStorageService _fileStorageService;
        private readonly FileStorageOptions _options;

        public AttachmentService(
            IUnitOfWork uow,
            IFileStorageService fileStorageService,
            IOptions<FileStorageOptions> options)
        {
            _uow = uow;
            _fileStorageService = fileStorageService;
            _options = options.Value;
        }

        public async Task<AttachmentDto> UploadAsync(IFormFile file, string entityName, Guid entityId, Guid uploadedByUserId)
        {
            var validEntities = typeof(AttachmentEntityType)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetRawConstantValue()?.ToString())
                .Where(v => v != null)
                .ToList();

            if (!validEntities.Contains(entityName))
            {
                throw new AppValidationException("Invalid entity name for attachment.");
            }

            if (file.Length > _options.MaxFileSizeBytes)
            {
                throw new AppValidationException("File exceeds the maximum allowed size.");
            }

            var extension = Path.GetExtension(file.FileName).ToLower();
            if (!_options.AllowedExtensions.Contains(extension))
            {
                throw new AppValidationException("File type is not allowed.");
            }

            using var stream = file.OpenReadStream();
            var storedFileName = await _fileStorageService.SaveAsync(stream, file.FileName);

            var attachment = new Attachment
            {
                EntityName = entityName,
                EntityId = entityId,
                FileName = file.FileName,
                StoredFileName = storedFileName,
                ContentType = file.ContentType,
                FileSizeBytes = file.Length,
                UploadedByUserId = uploadedByUserId
            };

            await _uow.Repository<Attachment>().AddAsync(attachment);
            await _uow.SaveChangesAsync();

            return MapToDto(attachment);
        }

        public async Task<(Stream stream, string contentType, string fileName)> DownloadAsync(Guid attachmentId)
        {
            var attachment = await _uow.Repository<Attachment>().GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                throw new NotFoundException(nameof(Attachment), attachmentId);
            }

            var stream = await _fileStorageService.GetFileStreamAsync(attachment.StoredFileName);
            return (stream, attachment.ContentType, attachment.FileName);
        }

        public async Task<List<AttachmentDto>> GetByEntityAsync(string entityName, Guid entityId)
        {
            var attachments = await _uow.Repository<Attachment>()
                .FindAsync(a => a.EntityName == entityName && a.EntityId == entityId && !a.IsDeleted);

            return attachments.Select(MapToDto).ToList();
        }

        public async Task DeleteAsync(Guid attachmentId)
        {
            var attachment = await _uow.Repository<Attachment>().GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                throw new NotFoundException(nameof(Attachment), attachmentId);
            }

            await _fileStorageService.DeleteAsync(attachment.StoredFileName);
            
            _uow.Repository<Attachment>().Remove(attachment);
            await _uow.SaveChangesAsync();
        }

        private static AttachmentDto MapToDto(Attachment attachment)
        {
            return new AttachmentDto
            {
                Id = attachment.Id,
                EntityName = attachment.EntityName,
                EntityId = attachment.EntityId,
                FileName = attachment.FileName,
                ContentType = attachment.ContentType,
                FileSizeBytes = attachment.FileSizeBytes,
                DownloadUrl = $"/api/v1/attachments/{attachment.Id}/download",
                UploadedByUserId = attachment.UploadedByUserId,
                CreatedAt = attachment.CreatedAt
            };
        }
    }
}
