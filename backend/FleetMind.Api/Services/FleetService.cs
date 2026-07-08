using System;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Services
{
    public class FleetService : IFleetService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public FleetService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<PagedResultDto<FleetDto>> GetFleetsAsync(FleetQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            
            var (items, totalCount) = await _uow.Fleets.GetPagedAsync(query);

            var fleetDtos = _mapper.Map<System.Collections.Generic.List<FleetDto>>(items);
            
            if (fleetDtos.Any())
            {
                var fleetIds = fleetDtos.Select(f => f.Id);
                var shipCounts = await _uow.Ships.GetShipCountsByFleetIdsAsync(fleetIds);
                foreach (var dto in fleetDtos)
                {
                    dto.ShipCount = shipCounts.GetValueOrDefault(dto.Id, 0);
                }
            }

            return new PagedResultDto<FleetDto>
            {
                Items = fleetDtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public async Task<FleetDto> GetFleetByIdAsync(Guid id)
        {
            var fleet = await _uow.Fleets.GetByIdAsync(id);
            if (fleet == null)
            {
                throw new NotFoundException(nameof(Fleet), id);
            }

            var fleetDto = _mapper.Map<FleetDto>(fleet);
            fleetDto.ShipCount = await _uow.Ships.CountByFleetIdAsync(fleet.Id);

            return fleetDto;
        }

        public async Task<FleetDto> CreateFleetAsync(CreateFleetDto dto)
        {
            var exists = await _uow.Fleets.ExistsByNameAsync(dto.Name);
            if (exists)
            {
                throw new ConflictException("A fleet with this name already exists.");
            }

            var fleet = _mapper.Map<Fleet>(dto);
            await _uow.Fleets.AddAsync(fleet);
            await _uow.SaveChangesAsync();

            return _mapper.Map<FleetDto>(fleet);
        }

        public async Task<FleetDto> UpdateFleetAsync(Guid id, UpdateFleetDto dto)
        {
            var fleet = await _uow.Fleets.GetByIdAsync(id);
            if (fleet == null)
            {
                throw new NotFoundException(nameof(Fleet), id);
            }

            var exists = await _uow.Fleets.ExistsByNameAsync(dto.Name, id);
            if (exists)
            {
                throw new ConflictException("A fleet with this name already exists.");
            }

            _mapper.Map(dto, fleet);
            _uow.Fleets.Update(fleet);
            await _uow.SaveChangesAsync();

            return _mapper.Map<FleetDto>(fleet);
        }

        public async Task<bool> DeactivateFleetAsync(Guid id)
        {
            var fleet = await _uow.Fleets.GetByIdAsync(id);
            if (fleet == null)
            {
                throw new NotFoundException(nameof(Fleet), id);
            }

            _uow.Fleets.Remove(fleet);
            await _uow.SaveChangesAsync();
            return true;
        }
    }
}
