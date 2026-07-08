using System;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Services.Ai.PromptBuilders;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.DTOs.Incidents;

namespace FleetMind.Api.Services
{
    public class VoyageService : IVoyageService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly INotificationRecipientResolver _recipientResolver;
        private readonly IAiProvider _aiProvider;

        public VoyageService(
            IUnitOfWork uow, 
            IMapper mapper,
            INotificationService notificationService,
            INotificationRecipientResolver recipientResolver,
            IAiProvider aiProvider)
        {
            _uow = uow;
            _mapper = mapper;
            _notificationService = notificationService;
            _recipientResolver = recipientResolver;
            _aiProvider = aiProvider;
        }

        public async Task<PagedResultDto<VoyageDto>> GetVoyagesAsync(VoyageQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            
            var (items, totalCount) = await _uow.Voyages.GetPagedAsync(query);

            return new PagedResultDto<VoyageDto>
            {
                Items = _mapper.Map<System.Collections.Generic.List<VoyageDto>>(items),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<VoyageDto> GetVoyageByIdAsync(Guid id)
        {
            var voyage = await _uow.Voyages.GetByIdWithShipAsync(id);
            if (voyage == null)
            {
                throw new NotFoundException(nameof(Voyage), id);
            }

            return _mapper.Map<VoyageDto>(voyage);
        }

        public async Task<VoyageDto> CreateVoyageAsync(CreateVoyageDto dto)
        {
            var exists = await _uow.Voyages.ExistsByVoyageNumberAsync(dto.VoyageNumber);
            if (exists)
            {
                throw new ConflictException("A voyage with this number already exists.");
            }

            var voyage = _mapper.Map<Voyage>(dto);
            voyage.Status = VoyageStatus.Scheduled;

            await _uow.Voyages.AddAsync(voyage);
            await _uow.SaveChangesAsync();

            // Re-fetch to populate Ship.Name
            var createdVoyage = await _uow.Voyages.GetByIdWithShipAsync(voyage.Id);
            return _mapper.Map<VoyageDto>(createdVoyage);
        }

        public async Task<VoyageDto> UpdateVoyageAsync(Guid id, UpdateVoyageDto dto)
        {
            var voyage = await _uow.Voyages.GetByIdWithShipAsync(id);
            if (voyage == null)
            {
                throw new NotFoundException(nameof(Voyage), id);
            }

            if (voyage.Status == VoyageStatus.Completed || voyage.Status == VoyageStatus.Cancelled)
            {
                throw new AppValidationException("Cannot modify a voyage that has already completed or been cancelled.");
            }

            _mapper.Map(dto, voyage);
            _uow.Voyages.Update(voyage);
            await _uow.SaveChangesAsync();

            return _mapper.Map<VoyageDto>(voyage);
        }

        public async Task DeleteVoyageAsync(Guid id)
        {
            var voyage = await _uow.Voyages.GetByIdAsync(id);
            if (voyage == null)
            {
                throw new NotFoundException(nameof(Voyage), id);
            }

            if (voyage.Status == VoyageStatus.InTransit)
            {
                throw new AppValidationException("Cannot delete a voyage that is currently in transit - cancel it first.");
            }

            _uow.Voyages.Remove(voyage);
            await _uow.SaveChangesAsync();
        }

        public async Task<VoyageDto> UpdateStatusAsync(Guid id, UpdateVoyageStatusDto dto)
        {
            var voyage = await _uow.Voyages.GetByIdWithShipAsync(id);
            if (voyage == null)
            {
                throw new NotFoundException(nameof(Voyage), id);
            }

            if (!VoyageStatusTransitions.IsLegalTransition(voyage.Status, dto.Status))
            {
                var legalNextStates = string.Join(", ", VoyageStatusTransitions.GetLegalNextStates(voyage.Status));
                throw new AppValidationException($"Cannot transition from '{voyage.Status}' to '{dto.Status}'. Legal next states are: {legalNextStates}");
            }

            voyage.Status = dto.Status;

            if (dto.Status == VoyageStatus.Completed)
            {
                voyage.ActualArrivalDate = dto.ActualArrivalDate ?? DateTime.UtcNow;
            }

            _uow.Voyages.Update(voyage);
            await _uow.SaveChangesAsync();

            if (dto.Status == VoyageStatus.Delayed)
            {
                var recipientIds = await _recipientResolver.GetUserIdsByRolesAsync(NotificationType.VoyageDelayed, AppRoles.Admin, AppRoles.FleetManager);
                var shipName = voyage.Ship?.Name ?? "Unknown Ship";
                
                foreach (var userId in recipientIds)
                {
                    await _notificationService.CreateAsync(
                        userId,
                        NotificationType.VoyageDelayed,
                        "Voyage Delayed",
                        $"Voyage {voyage.VoyageNumber} for ship {shipName} has been delayed.",
                        "Voyage",
                        voyage.Id
                    );
                }
            }

            return _mapper.Map<VoyageDto>(voyage);
        }

        public async Task<PagedResultDto<VoyageDto>> GetVoyagesForShipAsync(Guid shipId, VoyageQueryDto query)
        {
            query.ShipId = shipId;
            return await GetVoyagesAsync(query);
        }

        public async Task<List<Guid>> GetOverdueVoyageIdsAsync()
        {
            return await _uow.Voyages.GetOverdueVoyageIdsAsync();
        }

        public async Task<AiSummaryResultDto> GetAiSummaryAsync(Guid voyageId)
        {
            var voyageDto = await GetVoyageByIdAsync(voyageId);

            if (!_aiProvider.IsAvailable)
            {
                return new AiSummaryResultDto
                {
                    IsAvailable = false,
                    Summary = "AI features are currently unavailable in this environment.",
                    GeneratedAt = DateTime.UtcNow
                };
            }

            // Fetch related data
            var (cargoItems, _) = await _uow.Cargo.GetPagedAsync(new CargoQueryDto { VoyageId = voyageId, PageSize = 100 });
            var (incidents, _) = await _uow.Incidents.GetPagedAsync(new IncidentQueryDto { VoyageId = voyageId, PageSize = 100 });

            var cargoDtos = _mapper.Map<List<CargoDto>>(cargoItems);
            var incidentDtos = _mapper.Map<List<IncidentDto>>(incidents);

            var builder = new VoyageSummaryPromptBuilder();
            var systemPrompt = builder.BuildSystemPrompt();
            var userPrompt = builder.BuildUserPrompt(voyageDto, cargoDtos, incidentDtos);

            var result = await _aiProvider.CompleteAsync(new AiCompletionRequest
            {
                SystemPrompt = systemPrompt,
                UserPrompt = userPrompt
            });

            if (result.IsSuccess)
            {
                return new AiSummaryResultDto
                {
                    IsAvailable = true,
                    Summary = result.Content,
                    GeneratedAt = DateTime.UtcNow
                };
            }

            return new AiSummaryResultDto
            {
                IsAvailable = false,
                Summary = "Unable to generate a summary at this time.",
                GeneratedAt = DateTime.UtcNow
            };
        }
    }
}
