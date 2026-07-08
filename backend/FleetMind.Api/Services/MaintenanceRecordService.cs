using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Models;
using FleetMind.Api.DTOs.Common;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.Repositories;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common;

namespace FleetMind.Api.Services;

public class MaintenanceRecordService : IMaintenanceRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly INotificationRecipientResolver _recipientResolver;

    public MaintenanceRecordService(
        IUnitOfWork unitOfWork, 
        IMapper mapper,
        INotificationService notificationService,
        INotificationRecipientResolver recipientResolver)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _notificationService = notificationService;
        _recipientResolver = recipientResolver;
    }

    public async Task<PagedResultDto<MaintenanceRecordDto>> GetMaintenanceRecordsAsync(MaintenanceRecordQueryDto query)
    {
        query.PageSize = Math.Clamp(query.PageSize, 1, 100);
        var (items, totalCount) = await _unitOfWork.MaintenanceRecords.GetPagedAsync(query);
        
        return new PagedResultDto<MaintenanceRecordDto>
        {
            Items = _mapper.Map<List<MaintenanceRecordDto>>(items),
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize
        };
    }

    public async Task<MaintenanceRecordDto> GetMaintenanceRecordByIdAsync(Guid id)
    {
        var record = await _unitOfWork.MaintenanceRecords.GetByIdAsync(id);
        if (record == null || record.IsDeleted)
        {
            throw new NotFoundException(nameof(MaintenanceRecord), id);
        }

        // Load Ship for mapping ShipName
        if (record.Ship == null)
        {
            var ship = await _unitOfWork.Ships.GetByIdAsync(record.ShipId);
            if (ship != null)
            {
                record.Ship = ship;
            }
        }

        return _mapper.Map<MaintenanceRecordDto>(record);
    }

    public async Task<MaintenanceRecordDto> CreateMaintenanceRecordAsync(CreateMaintenanceRecordDto dto)
    {
        var record = new MaintenanceRecord
        {
            ShipId = dto.ShipId,
            Type = dto.Type,
            Description = dto.Description,
            ScheduledDate = dto.ScheduledDate,
            EstimatedCost = dto.EstimatedCost,
            PerformedBy = dto.PerformedBy,
            Status = MaintenanceStatus.Scheduled // Always starts Scheduled
        };

        await _unitOfWork.MaintenanceRecords.AddAsync(record);
        await _unitOfWork.SaveChangesAsync();

        return await GetMaintenanceRecordByIdAsync(record.Id);
    }

    public async Task<MaintenanceRecordDto> UpdateMaintenanceRecordAsync(Guid id, UpdateMaintenanceRecordDto dto)
    {
        var record = await _unitOfWork.MaintenanceRecords.GetByIdAsync(id);
        if (record == null || record.IsDeleted)
        {
            throw new NotFoundException(nameof(MaintenanceRecord), id);
        }

        if (record.Status == MaintenanceStatus.Completed || record.Status == MaintenanceStatus.Cancelled)
        {
            throw new AppValidationException("Cannot modify a maintenance record that is already Completed or Cancelled.");
        }

        record.Description = dto.Description;
        record.ScheduledDate = dto.ScheduledDate;
        record.EstimatedCost = dto.EstimatedCost;
        record.ActualCost = dto.ActualCost;
        record.PerformedBy = dto.PerformedBy;

        _unitOfWork.MaintenanceRecords.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return await GetMaintenanceRecordByIdAsync(id);
    }

    public async Task DeleteMaintenanceRecordAsync(Guid id)
    {
        var record = await _unitOfWork.MaintenanceRecords.GetByIdAsync(id);
        if (record == null || record.IsDeleted)
        {
            throw new NotFoundException(nameof(MaintenanceRecord), id);
        }

        if (record.Status == MaintenanceStatus.InProgress)
        {
            throw new AppValidationException("Cannot delete a maintenance record that is currently InProgress. Please cancel it first.");
        }

        _unitOfWork.MaintenanceRecords.Remove(record);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<MaintenanceRecordDto> UpdateStatusAsync(Guid id, UpdateMaintenanceStatusDto dto)
    {
        var record = await _unitOfWork.MaintenanceRecords.GetByIdAsync(id);
        if (record == null || record.IsDeleted)
        {
            throw new NotFoundException(nameof(MaintenanceRecord), id);
        }

        if (!MaintenanceStatusTransitions.IsLegalTransition(record.Status, dto.Status))
        {
            var allowedStates = string.Join(", ", MaintenanceStatusTransitions.GetLegalNextStates(record.Status));
            var allowedMessage = string.IsNullOrEmpty(allowedStates) ? "none" : allowedStates;
            throw new AppValidationException($"Invalid status transition from {record.Status} to {dto.Status}. Legal next states: {allowedMessage}");
        }

        record.Status = dto.Status;

        if (dto.Status == MaintenanceStatus.Completed)
        {
            record.CompletedDate = dto.CompletedDate ?? DateTime.UtcNow;
            if (dto.ActualCost.HasValue)
            {
                record.ActualCost = dto.ActualCost.Value;
            }
        }

        _unitOfWork.MaintenanceRecords.Update(record);
        await _unitOfWork.SaveChangesAsync();

        return await GetMaintenanceRecordByIdAsync(id);
    }

    public async Task<PagedResultDto<MaintenanceRecordDto>> GetMaintenanceForShipAsync(Guid shipId, MaintenanceRecordQueryDto query)
    {
        query.ShipId = shipId;
        return await GetMaintenanceRecordsAsync(query);
    }

    public async Task<List<Guid>> GetOverdueRecordIdsAsync()
    {
        return await _unitOfWork.MaintenanceRecords.GetOverdueRecordIdsAsync();
    }

    public async Task MarkOverdueAsync(Guid id)
    {
        var record = await _unitOfWork.MaintenanceRecords.GetByIdAsync(id);
        if (record == null || record.IsDeleted) return;

        // Bypassing the public graph checks
        record.Status = MaintenanceStatus.Overdue;
        
        _unitOfWork.MaintenanceRecords.Update(record);
        await _unitOfWork.SaveChangesAsync();

        // Send notification
        var ship = record.Ship ?? await _unitOfWork.Ships.GetByIdAsync(record.ShipId);
        var shipName = ship?.Name ?? "Unknown Ship";

        var recipientIds = await _recipientResolver.GetUserIdsByRolesAsync(NotificationType.MaintenanceOverdue, AppRoles.Admin, AppRoles.MaintenanceOfficer);
        foreach (var userId in recipientIds)
        {
            await _notificationService.CreateAsync(
                userId,
                NotificationType.MaintenanceOverdue,
                "Maintenance Overdue",
                $"Maintenance '{record.Description}' for ship {shipName} is now overdue.",
                "MaintenanceRecord",
                record.Id
            );
        }
    }
}
