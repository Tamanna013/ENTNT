using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Services
{
    public class FuelLogService : IFuelLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IReportingService _reportingService;
        private readonly INotificationService _notificationService;
        private readonly INotificationRecipientResolver _recipientResolver;

        public FuelLogService(
            IUnitOfWork unitOfWork, 
            IMapper mapper,
            IReportingService reportingService,
            INotificationService notificationService,
            INotificationRecipientResolver recipientResolver)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _reportingService = reportingService;
            _notificationService = notificationService;
            _recipientResolver = recipientResolver;
        }

        public async Task<PagedResultDto<FuelLogDto>> GetFuelLogsAsync(FuelLogQueryDto query)
        {
            query.PageSize = Math.Clamp(query.PageSize, 1, 100);
            var (items, totalCount) = await _unitOfWork.FuelLogs.GetPagedAsync(query);
            var dtos = _mapper.Map<List<FuelLogDto>>(items);

            return new PagedResultDto<FuelLogDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                PageNumber = query.PageNumber,
                PageSize = query.PageSize
            };
        }

        public Task<PagedResultDto<FuelLogDto>> GetFuelLogsForShipAsync(Guid shipId, FuelLogQueryDto query)
        {
            query.ShipId = shipId;
            return GetFuelLogsAsync(query);
        }

        public async Task<FuelLogDto> GetFuelLogByIdAsync(Guid id)
        {
            var fuelLog = await _unitOfWork.FuelLogs.GetByIdAsync(id);
            if (fuelLog == null)
                throw new NotFoundException(nameof(FuelLog), id);

            // Fetch relations manually if not included by GetByIdAsync (GetByIdAsync usually doesn't include navigations)
            fuelLog.Ship = await _unitOfWork.Ships.GetByIdAsync(fuelLog.ShipId) ?? fuelLog.Ship;
            if (fuelLog.VoyageId.HasValue)
                fuelLog.Voyage = await _unitOfWork.Voyages.GetByIdAsync(fuelLog.VoyageId.Value) ?? fuelLog.Voyage;

            return _mapper.Map<FuelLogDto>(fuelLog);
        }

        public async Task<FuelLogDto> CreateFuelLogAsync(CreateFuelLogDto dto)
        {
            var fuelLog = _mapper.Map<FuelLog>(dto);
            
            // Check for fuel anomaly BEFORE saving the new log
            var efficiencyReport = await _reportingService.GetFuelEfficiencyReportAsync(90);
            var shipReport = efficiencyReport.Find(r => r.ShipId == fuelLog.ShipId);
            
            bool isAnomaly = false;
            decimal previousAverage = 0m;
            decimal newCostPerLiter = fuelLog.CostPerLiter;

            if (shipReport != null && shipReport.LogCount > 0)
            {
                previousAverage = shipReport.AverageCostPerLiter;
                if (newCostPerLiter > previousAverage * 1.5m)
                {
                    isAnomaly = true;
                }
            }

            await _unitOfWork.FuelLogs.AddAsync(fuelLog);
            await _unitOfWork.SaveChangesAsync();

            if (isAnomaly)
            {
                var recipientIds = await _recipientResolver.GetUserIdsByRolesAsync(NotificationType.FuelAnomaly, AppRoles.Admin, AppRoles.FleetManager);
                var ship = await _unitOfWork.Ships.GetByIdAsync(fuelLog.ShipId);
                var shipName = ship?.Name ?? "Unknown Ship";

                foreach (var userId in recipientIds)
                {
                    await _notificationService.CreateAsync(
                        userId,
                        NotificationType.FuelAnomaly,
                        "Fuel Cost Anomaly",
                        $"Fuel log for ship {shipName} has a cost per liter of {newCostPerLiter:C} which exceeds the 90-day average of {previousAverage:C} by >50%.",
                        "FuelLog",
                        fuelLog.Id
                    );
                }
            }

            return await GetFuelLogByIdAsync(fuelLog.Id);
        }

        public async Task<FuelLogDto> UpdateFuelLogAsync(Guid id, UpdateFuelLogDto dto)
        {
            var fuelLog = await _unitOfWork.FuelLogs.GetByIdAsync(id);
            if (fuelLog == null)
                throw new NotFoundException(nameof(FuelLog), id);

            _mapper.Map(dto, fuelLog);
            
            _unitOfWork.FuelLogs.Update(fuelLog);
            await _unitOfWork.SaveChangesAsync();

            return await GetFuelLogByIdAsync(fuelLog.Id);
        }

        public async Task DeleteFuelLogAsync(Guid id)
        {
            var fuelLog = await _unitOfWork.FuelLogs.GetByIdAsync(id);
            if (fuelLog == null)
                throw new NotFoundException(nameof(FuelLog), id);

            _unitOfWork.FuelLogs.Remove(fuelLog);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
