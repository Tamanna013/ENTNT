using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace FleetMind.Api.Services
{
    public class ContainerService : IContainerService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public ContainerService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<ContainerDto>> GetContainersAsync(ContainerQueryDto query)
        {
            var (items, totalCount) = await _uow.Containers.GetPagedAsync(query);

            return new PagedResultDto<ContainerDto>
            {
                Items = _mapper.Map<List<ContainerDto>>(items),
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<ContainerDto> GetContainerByIdAsync(Guid id)
        {
            var container = await _uow.Containers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Container with ID {id} not found.");

            // Flatten Voyage if it exists
            if (container.CurrentVoyageId.HasValue)
            {
                container.CurrentVoyage = await _uow.Voyages.GetByIdAsync(container.CurrentVoyageId.Value);
            }

            // Fetch cargo items to populate LinkedCargoIds
            var cargoItems = await _uow.Context.ContainerCargoItems
                .Where(cci => cci.ContainerId == id)
                .ToListAsync();
            container.ContainerCargoItems = cargoItems;

            return _mapper.Map<ContainerDto>(container);
        }

        public async Task<ContainerDto> CreateContainerAsync(CreateContainerDto dto)
        {
            if (await _uow.Containers.ExistsByContainerNumberAsync(dto.ContainerNumber))
            {
                throw new ConflictException("A container with this number already exists.");
            }

            var container = _mapper.Map<Container>(dto);
            container.Status = ContainerStatus.Empty;

            await _uow.Containers.AddAsync(container);
            await _uow.SaveChangesAsync();

            return await GetContainerByIdAsync(container.Id); // re-fetch to ensure relations are mapped correctly
        }

        public async Task<ContainerDto> UpdateContainerAsync(Guid id, UpdateContainerDto dto)
        {
            var container = await _uow.Containers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Container with ID {id} not found.");

            container.Type = dto.Type;
            container.Status = dto.Status;
            container.CurrentVoyageId = dto.CurrentVoyageId;

            _uow.Containers.Update(container);
            await _uow.SaveChangesAsync();

            return await GetContainerByIdAsync(id);
        }

        public async Task DeleteContainerAsync(Guid id)
        {
            var container = await _uow.Containers.GetByIdAsync(id)
                ?? throw new NotFoundException($"Container with ID {id} not found.");

            container.IsDeleted = true;
            _uow.Containers.Update(container);
            await _uow.SaveChangesAsync();
        }

        public async Task<ContainerDto> LinkCargoAsync(Guid containerId, Guid cargoId)
        {
            var container = await _uow.Containers.GetByIdAsync(containerId)
                ?? throw new NotFoundException($"Container with ID {containerId} not found.");

            var cargo = await _uow.Cargo.GetByIdAsync(cargoId)
                ?? throw new NotFoundException($"Cargo item with ID {cargoId} not found.");

            var existingLink = await _uow.Context.ContainerCargoItems
                .FirstOrDefaultAsync(cci => cci.ContainerId == containerId && cci.CargoId == cargoId);

            if (existingLink == null)
            {
                _uow.Context.ContainerCargoItems.Add(new ContainerCargoItem
                {
                    ContainerId = containerId,
                    CargoId = cargoId
                });
                await _uow.SaveChangesAsync();
            }

            return await GetContainerByIdAsync(containerId);
        }

        public async Task<ContainerDto> UnlinkCargoAsync(Guid containerId, Guid cargoId)
        {
            var container = await _uow.Containers.GetByIdAsync(containerId)
                ?? throw new NotFoundException($"Container with ID {containerId} not found.");

            var link = await _uow.Context.ContainerCargoItems
                .FirstOrDefaultAsync(cci => cci.ContainerId == containerId && cci.CargoId == cargoId);

            if (link != null)
            {
                _uow.Context.ContainerCargoItems.Remove(link);
                await _uow.SaveChangesAsync();
            }

            return await GetContainerByIdAsync(containerId);
        }

        public async Task<ContainerTrackingEventDto> RecordTrackingEventAsync(Guid containerId, RecordTrackingEventDto dto, Guid recordedByUserId)
        {
            var container = await _uow.Containers.GetByIdAsync(containerId)
                ?? throw new NotFoundException($"Container with ID {containerId} not found.");

            var trackingEvent = new ContainerTrackingEvent
            {
                ContainerId = containerId,
                EventType = dto.EventType,
                Location = dto.Location,
                Timestamp = dto.Timestamp,
                Notes = dto.Notes,
                RecordedByUserId = recordedByUserId
            };

            await _uow.Context.ContainerTrackingEvents.AddAsync(trackingEvent);
            await _uow.SaveChangesAsync();

            var eventDto = _mapper.Map<ContainerTrackingEventDto>(trackingEvent);

            // Attempt to resolve User Name
            var user = await _uow.Users.GetByIdAsync(recordedByUserId);
            eventDto.RecordedByUserName = user != null ? $"{user.FirstName} {user.LastName}".Trim() : "Unknown User";

            return eventDto;
        }

        public async Task<List<ContainerTrackingEventDto>> GetTrackingEventsAsync(Guid containerId)
        {
            var container = await _uow.Containers.GetByIdAsync(containerId)
                ?? throw new NotFoundException($"Container with ID {containerId} not found.");

            var events = await _uow.Containers.GetTrackingEventsAsync(containerId);
            var dtos = _mapper.Map<List<ContainerTrackingEventDto>>(events);

            // Bulk resolve User Names
            var userIds = events.Select(e => e.RecordedByUserId).Distinct().ToList();

            var userDict = await _uow.Context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

            foreach (var dto in dtos)
            {
                if (userDict.TryGetValue(dto.RecordedByUserId, out var name))
                {
                    dto.RecordedByUserName = name;
                }
                else
                {
                    dto.RecordedByUserName = "Unknown User";
                }
            }

            return dtos;
        }
    }
}
