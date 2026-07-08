using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.DTOs.Ai;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Services.Ai.PromptBuilders;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly INotificationRecipientResolver _recipientResolver;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAiProvider _aiProvider;

        public IncidentService(
            IUnitOfWork uow, 
            IMapper mapper,
            INotificationService notificationService,
            INotificationRecipientResolver recipientResolver,
            ICurrentUserService currentUserService,
            IAiProvider aiProvider)
        {
            _uow = uow;
            _mapper = mapper;
            _notificationService = notificationService;
            _recipientResolver = recipientResolver;
            _currentUserService = currentUserService;
            _aiProvider = aiProvider;
        }

        public async Task<PagedResultDto<IncidentDto>> GetIncidentsAsync(IncidentQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            var (items, totalCount) = await _uow.Incidents.GetPagedAsync(query);

            return new PagedResultDto<IncidentDto>
            {
                Items = _mapper.Map<List<IncidentDto>>(items),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<IncidentDto> GetIncidentByIdAsync(Guid id)
        {
            // We need to fetch with navigations so we map it similarly to the paged version.
            // Using the repository FindAsync to easily eagerly load using the underlying logic, or manual.
            var items = await _uow.Context.Incidents
                .Include(i => i.Ship)
                .Include(i => i.Voyage)
                .Include(i => i.ReportedByUser)
                .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);

            if (items == null)
            {
                throw new NotFoundException(nameof(Incident), id);
            }

            return _mapper.Map<IncidentDto>(items);
        }

        public async Task<AiSummaryResultDto> GetAiReportAsync(Guid incidentId)
        {
            var incidentDto = await GetIncidentByIdAsync(incidentId);

            if (!_aiProvider.IsAvailable)
            {
                return new AiSummaryResultDto
                {
                    IsAvailable = false,
                    Summary = "AI features are currently unavailable in this environment.",
                    GeneratedAt = DateTime.UtcNow
                };
            }

            var builder = new IncidentReportPromptBuilder();
            var systemPrompt = builder.BuildSystemPrompt();
            var userPrompt = builder.BuildUserPrompt(incidentDto);

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
                Summary = "Unable to generate a report at this time.",
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<IncidentDto> CreateIncidentAsync(CreateIncidentDto dto)
        {
            var userId = _currentUserService.UserId;
            if (!userId.HasValue) throw new UnauthorizedAccessException();

            var incident = _mapper.Map<Incident>(dto);
            incident.ReportedByUserId = userId.Value;
            incident.Status = IncidentStatus.Reported;

            await _uow.Incidents.AddAsync(incident);
            await _uow.SaveChangesAsync();

            // Notify Admin if High or Critical
            if (dto.Severity == IncidentSeverity.High || dto.Severity == IncidentSeverity.Critical)
            {
                var recipientIds = await _recipientResolver.GetUserIdsByRolesAsync(NotificationType.IncidentReported, AppRoles.Admin);
                var ship = await _uow.Ships.GetByIdAsync(incident.ShipId);
                var shipName = ship?.Name ?? "Unknown Ship";

                foreach (var recipientId in recipientIds)
                {
                    await _notificationService.CreateAsync(
                        recipientId,
                        NotificationType.IncidentReported,
                        "Critical Incident Reported",
                        $"A {dto.Severity} severity incident '{dto.Title}' was reported for ship {shipName}.",
                        "Incident",
                        incident.Id
                    );
                }
            }

            return await GetIncidentByIdAsync(incident.Id);
        }

        public async Task<IncidentDto> UpdateIncidentAsync(Guid id, UpdateIncidentDto dto)
        {
            var incident = await _uow.Incidents.GetByIdAsync(id);
            if (incident == null)
            {
                throw new NotFoundException(nameof(Incident), id);
            }

            if (incident.Status == IncidentStatus.Closed)
            {
                throw new AppValidationException("Cannot modify a closed incident.");
            }

            _mapper.Map(dto, incident);
            _uow.Incidents.Update(incident);
            await _uow.SaveChangesAsync();

            return await GetIncidentByIdAsync(id);
        }

        public async Task DeleteIncidentAsync(Guid id)
        {
            var incident = await _uow.Incidents.GetByIdAsync(id);
            if (incident == null)
            {
                throw new NotFoundException(nameof(Incident), id);
            }

            // Soft-delete directly without status-based block
            _uow.Incidents.Remove(incident);
            await _uow.SaveChangesAsync();
        }

        public async Task<IncidentDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusDto dto)
        {
            var incident = await _uow.Incidents.GetByIdAsync(id);
            if (incident == null)
            {
                throw new NotFoundException(nameof(Incident), id);
            }

            if (!IncidentStatusTransitions.IsLegalTransition(incident.Status, dto.Status))
            {
                var allowedStates = string.Join(", ", IncidentStatusTransitions.GetLegalNextStates(incident.Status));
                var allowedMessage = string.IsNullOrEmpty(allowedStates) ? "none" : allowedStates;
                throw new AppValidationException($"Invalid status transition from {incident.Status} to {dto.Status}. Legal next states: {allowedMessage}");
            }

            incident.Status = dto.Status;

            if (dto.Status == IncidentStatus.Resolved)
            {
                incident.ResolvedAt = dto.ResolvedAt ?? DateTime.UtcNow;
                if (!string.IsNullOrWhiteSpace(dto.ResolutionNotes))
                {
                    incident.ResolutionNotes = dto.ResolutionNotes;
                }
            }

            _uow.Incidents.Update(incident);
            await _uow.SaveChangesAsync();

            return await GetIncidentByIdAsync(id);
        }

        public async Task<PagedResultDto<IncidentDto>> GetIncidentsForShipAsync(Guid shipId, IncidentQueryDto query)
        {
            query.ShipId = shipId;
            return await GetIncidentsAsync(query);
        }
    }
}
