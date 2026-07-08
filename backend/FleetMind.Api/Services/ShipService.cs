using System;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.DTOs.Attachments;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Models;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Services.Ai.PromptBuilders;
using FleetMind.Api.DTOs.Maintenance;
using System.Linq;
using FleetMind.Api.Common;

namespace FleetMind.Api.Services
{
    public class ShipService : IShipService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IAttachmentService _attachmentService;
        private readonly IAiProvider _aiProvider;

        public ShipService(IUnitOfWork uow, IMapper mapper, IAttachmentService attachmentService, IAiProvider aiProvider)
        {
            _uow = uow;
            _mapper = mapper;
            _attachmentService = attachmentService;
            _aiProvider = aiProvider;
        }

        public async Task<PagedResultDto<ShipDto>> GetShipsAsync(ShipQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            
            var (items, totalCount) = await _uow.Ships.GetPagedAsync(query);

            return new PagedResultDto<ShipDto>
            {
                Items = _mapper.Map<System.Collections.Generic.List<ShipDto>>(items),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<ShipDto> GetShipByIdAsync(Guid id)
        {
            var ship = await _uow.Ships.GetByIdWithFleetAsync(id);
            if (ship == null)
            {
                throw new NotFoundException(nameof(Ship), id);
            }

            return _mapper.Map<ShipDto>(ship);
        }

        public async Task<ShipDto> CreateShipAsync(CreateShipDto dto)
        {
            var exists = await _uow.Ships.ExistsByImoAsync(dto.IMO);
            if (exists)
            {
                throw new ConflictException("A ship with this IMO number already exists.");
            }

            var ship = _mapper.Map<Ship>(dto);
            await _uow.Ships.AddAsync(ship);
            await _uow.SaveChangesAsync();

            // Fetch with Fleet to populate FleetName
            var createdShip = await _uow.Ships.GetByIdWithFleetAsync(ship.Id);
            return _mapper.Map<ShipDto>(createdShip);
        }

        public async Task<ShipDto> UpdateShipAsync(Guid id, UpdateShipDto dto)
        {
            var ship = await _uow.Ships.GetByIdWithFleetAsync(id);
            if (ship == null)
            {
                throw new NotFoundException(nameof(Ship), id);
            }

            _mapper.Map(dto, ship);
            _uow.Ships.Update(ship);
            await _uow.SaveChangesAsync();

            return _mapper.Map<ShipDto>(ship);
        }

        public async Task<bool> DeactivateShipAsync(Guid id)
        {
            var ship = await _uow.Ships.GetByIdAsync(id);
            if (ship == null)
            {
                throw new NotFoundException(nameof(Ship), id);
            }

            _uow.Ships.Remove(ship);
            await _uow.SaveChangesAsync();
            return true;
        }

        public async Task<System.Collections.Generic.List<AttachmentDto>> GetAttachmentsAsync(Guid shipId)
        {
            var ship = await _uow.Ships.GetByIdAsync(shipId);
            if (ship == null)
            {
                throw new NotFoundException(nameof(Ship), shipId);
            }

            return await _attachmentService.GetByEntityAsync(AttachmentEntityType.Ship, shipId);
        }

        public async Task<AttachmentDto> UploadAttachmentAsync(Guid shipId, Microsoft.AspNetCore.Http.IFormFile file, Guid uploadedByUserId)
        {
            var ship = await _uow.Ships.GetByIdAsync(shipId);
            if (ship == null)
            {
                throw new NotFoundException(nameof(Ship), shipId);
            }

            return await _attachmentService.UploadAsync(file, AttachmentEntityType.Ship, shipId, uploadedByUserId);
        }

        public async Task<ShipDto> SetPrimaryPhotoAsync(Guid shipId, Guid attachmentId)
        {
            var ship = await _uow.Ships.GetByIdWithFleetAsync(shipId);
            if (ship == null)
            {
                throw new NotFoundException(nameof(Ship), shipId);
            }

            var attachment = await _uow.Repository<Attachment>().GetByIdAsync(attachmentId);
            if (attachment == null)
            {
                throw new NotFoundException(nameof(Attachment), attachmentId);
            }

            if (attachment.EntityName != AttachmentEntityType.Ship || attachment.EntityId != shipId)
            {
                throw new AppValidationException("The specified attachment does not belong to this ship.");
            }

            ship.PrimaryPhotoAttachmentId = attachmentId;
            await _uow.SaveChangesAsync();

            return _mapper.Map<ShipDto>(ship);
        }

        public async Task<AiRecommendationResultDto> GetAiMaintenanceRecommendationsAsync(Guid shipId)
        {
            const string disclaimer = "These are informational suggestions based on historical patterns, not a substitute for professional maintenance judgment.";
            
            var ship = await GetShipByIdAsync(shipId);
            
            if (!_aiProvider.IsAvailable)
            {
                return new AiRecommendationResultDto
                {
                    IsAvailable = false,
                    Recommendations = new System.Collections.Generic.List<string>(),
                    GeneratedAt = DateTime.UtcNow,
                    Disclaimer = disclaimer
                };
            }

            var query = new MaintenanceRecordQueryDto 
            { 
                ShipId = shipId, 
                Status = "Completed",
                PageSize = 50,
                SortDescending = false, // Chronological
                SortBy = "occurredat" // Let's use standard OccurredAt or CreatedAt if Scheduled/Completed isn't in generic sort
            };

            var (maintenanceRecords, _) = await _uow.MaintenanceRecords.GetPagedAsync(query);
            // In memory chronological sort by CompletedDate to be safe
            var sortedRecords = maintenanceRecords.Where(x => x.CompletedDate.HasValue).OrderBy(x => x.CompletedDate).ToList();
            var recordDtos = _mapper.Map<System.Collections.Generic.List<MaintenanceRecordDto>>(sortedRecords);

            var builder = new MaintenanceRecommendationPromptBuilder();
            var result = await _aiProvider.CompleteAsync(new AiCompletionRequest
            {
                SystemPrompt = builder.BuildSystemPrompt(),
                UserPrompt = builder.BuildUserPrompt(ship, recordDtos)
            });

            if (result.IsSuccess)
            {
                var recommendations = result.Content
                    .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => x.TrimStart(' ', '-', '*', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.'))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                return new AiRecommendationResultDto
                {
                    IsAvailable = true,
                    Recommendations = recommendations,
                    GeneratedAt = DateTime.UtcNow,
                    Disclaimer = disclaimer
                };
            }

            return new AiRecommendationResultDto
            {
                IsAvailable = false,
                Recommendations = new System.Collections.Generic.List<string>(),
                GeneratedAt = DateTime.UtcNow,
                Disclaimer = disclaimer
            };
        }
    }
}
