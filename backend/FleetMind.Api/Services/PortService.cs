using System;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Ports;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace FleetMind.Api.Services
{
    public class PortService : IPortService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ILogger<PortService> _logger;

        public PortService(IUnitOfWork uow, IMapper mapper, ILogger<PortService> logger)
        {
            _uow = uow;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PagedResultDto<PortDto>> GetPortsAsync(PortQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            var (items, totalCount) = await _uow.Ports.GetPagedAsync(query);
            var dtos = _mapper.Map<System.Collections.Generic.List<PortDto>>(items);
            return new PagedResultDto<PortDto>
            {
                Items = dtos,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<PortDto> GetPortByIdAsync(Guid id)
        {
            var port = await _uow.Ports.GetByIdAsync(id);
            if (port == null)
            {
                throw new NotFoundException(nameof(Port), id);
            }
            return _mapper.Map<PortDto>(port);
        }

        public async Task<PortDto> CreatePortAsync(CreatePortDto dto)
        {
            if (await _uow.Ports.ExistsByUnLocodeAsync(dto.UnLocode))
            {
                throw new ConflictException($"A port with UN/LOCODE '{dto.UnLocode}' already exists.");
            }

            var port = _mapper.Map<Port>(dto);
            
            await _uow.Ports.AddAsync(port);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Created new port {PortId} with UN/LOCODE {UnLocode}", port.Id, port.UnLocode);
            
            return _mapper.Map<PortDto>(port);
        }

        public async Task<PortDto> UpdatePortAsync(Guid id, UpdatePortDto dto)
        {
            var port = await _uow.Ports.GetByIdAsync(id);
            if (port == null)
            {
                throw new NotFoundException(nameof(Port), id);
            }

            _mapper.Map(dto, port);
            
            _uow.Ports.Update(port);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Updated port {PortId}", port.Id);

            return _mapper.Map<PortDto>(port);
        }

        public async Task DeactivatePortAsync(Guid id)
        {
            var port = await _uow.Ports.GetByIdAsync(id);
            if (port == null)
            {
                throw new NotFoundException(nameof(Port), id);
            }

            _uow.Ports.Remove(port);
            await _uow.SaveChangesAsync();

            _logger.LogInformation("Soft-deleted port {PortId}", port.Id);
        }
    }
}
