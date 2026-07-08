using System;
using System.Threading.Tasks;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.DTOs.Attachments;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Services
{
    public interface IShipService
    {
        Task<PagedResultDto<ShipDto>> GetShipsAsync(ShipQueryDto query);
        Task<ShipDto> GetShipByIdAsync(Guid id);
        Task<ShipDto> CreateShipAsync(CreateShipDto dto);
        Task<ShipDto> UpdateShipAsync(Guid id, UpdateShipDto dto);
        Task<bool> DeactivateShipAsync(Guid id);
        
        Task<System.Collections.Generic.List<AttachmentDto>> GetAttachmentsAsync(Guid shipId);
        Task<AttachmentDto> UploadAttachmentAsync(Guid shipId, Microsoft.AspNetCore.Http.IFormFile file, Guid uploadedByUserId);
        Task<ShipDto> SetPrimaryPhotoAsync(Guid shipId, Guid attachmentId);
        Task<AiRecommendationResultDto> GetAiMaintenanceRecommendationsAsync(Guid shipId);
    }
}
