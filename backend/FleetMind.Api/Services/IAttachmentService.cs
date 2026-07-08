using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Attachments;
using Microsoft.AspNetCore.Http;

namespace FleetMind.Api.Services
{
    public interface IAttachmentService
    {
        Task<AttachmentDto> UploadAsync(IFormFile file, string entityName, Guid entityId, Guid uploadedByUserId);
        Task<(Stream stream, string contentType, string fileName)> DownloadAsync(Guid attachmentId);
        Task<List<AttachmentDto>> GetByEntityAsync(string entityName, Guid entityId);
        Task DeleteAsync(Guid attachmentId);
    }
}
