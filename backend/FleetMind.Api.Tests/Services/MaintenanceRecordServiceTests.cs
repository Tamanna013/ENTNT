using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FleetMind.Api.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetMind.Api.Tests.Services;

public class MaintenanceRecordServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IMaintenanceRecordRepository> _recordRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<INotificationRecipientResolver> _recipientMock;
    private readonly MaintenanceRecordService _service;

    public MaintenanceRecordServiceTests()
    {
        _uowMock = MockUnitOfWorkFactory.CreateDefault();
        _recordRepoMock = new Mock<IMaintenanceRecordRepository>();
        _uowMock.Setup(u => u.MaintenanceRecords).Returns(_recordRepoMock.Object);

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<MaintenanceRecord, MaintenanceRecordDto>();
            cfg.CreateMap<Ship, FleetMind.Api.DTOs.Ships.ShipDto>();
        });
        _mapper = config.CreateMapper();

        _notificationMock = new Mock<INotificationService>();
        _recipientMock = new Mock<INotificationRecipientResolver>();

        _service = new MaintenanceRecordService(
            _uowMock.Object,
            _mapper,
            _notificationMock.Object,
            _recipientMock.Object
        );
    }

    private (Guid Id, MaintenanceRecord Record) SetupRecordMock(string initialStatus)
    {
        var recordId = Guid.NewGuid();
        var record = new MaintenanceRecord { Id = recordId, Status = initialStatus, ShipId = Guid.NewGuid() };
        _recordRepoMock.Setup(r => r.GetByIdAsync(recordId)).ReturnsAsync(record);
        
        var mockShip = new Ship { Id = record.ShipId, Name = "Test Ship" };
        _uowMock.Setup(u => u.Ships.GetByIdAsync(record.ShipId)).ReturnsAsync(mockShip);
        record.Ship = mockShip;

        return (recordId, record);
    }

    [Fact]
    public async Task MarkOverdueAsync_CorrectlySetsStatusToOverdue()
    {
        var mock = SetupRecordMock(MaintenanceStatus.Scheduled);
        _recipientMock.Setup(r => r.GetUserIdsByRolesAsync(It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new List<Guid>());

        await _service.MarkOverdueAsync(mock.Id);

        mock.Record.Status.Should().Be(MaintenanceStatus.Overdue);
        _recordRepoMock.Verify(r => r.Update(mock.Record), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_RejectsManualOverdueTransition()
    {
        var mock = SetupRecordMock(MaintenanceStatus.Scheduled);
        var dto = new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Overdue };

        var act = async () => await _service.UpdateStatusAsync(mock.Id, dto);

        await act.Should().ThrowAsync<AppValidationException>()
            .WithMessage("*Invalid status transition*");
    }

    [Fact]
    public async Task UpdateStatusAsync_EnforcesLegalTransitions()
    {
        // Scheduled -> InProgress
        var scheduledMock = SetupRecordMock(MaintenanceStatus.Scheduled);
        await _service.UpdateStatusAsync(scheduledMock.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.InProgress });
        scheduledMock.Record.Status.Should().Be(MaintenanceStatus.InProgress);

        // Scheduled -> Cancelled
        var scheduledMock2 = SetupRecordMock(MaintenanceStatus.Scheduled);
        await _service.UpdateStatusAsync(scheduledMock2.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Cancelled });
        scheduledMock2.Record.Status.Should().Be(MaintenanceStatus.Cancelled);

        // InProgress -> Completed
        var inProgressMock = SetupRecordMock(MaintenanceStatus.InProgress);
        await _service.UpdateStatusAsync(inProgressMock.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Completed });
        inProgressMock.Record.Status.Should().Be(MaintenanceStatus.Completed);

        // InProgress -> Cancelled
        var inProgressMock2 = SetupRecordMock(MaintenanceStatus.InProgress);
        await _service.UpdateStatusAsync(inProgressMock2.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Cancelled });
        inProgressMock2.Record.Status.Should().Be(MaintenanceStatus.Cancelled);

        // Overdue -> InProgress
        var overdueMock = SetupRecordMock(MaintenanceStatus.Overdue);
        await _service.UpdateStatusAsync(overdueMock.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.InProgress });
        overdueMock.Record.Status.Should().Be(MaintenanceStatus.InProgress);

        // Overdue -> Cancelled
        var overdueMock2 = SetupRecordMock(MaintenanceStatus.Overdue);
        await _service.UpdateStatusAsync(overdueMock2.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Cancelled });
        overdueMock2.Record.Status.Should().Be(MaintenanceStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateStatusAsync_RejectsIllegalTransitions()
    {
        // Scheduled -> Completed
        var scheduledMock = SetupRecordMock(MaintenanceStatus.Scheduled);
        var act1 = async () => await _service.UpdateStatusAsync(scheduledMock.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Completed });
        await act1.Should().ThrowAsync<AppValidationException>();

        // Overdue -> Completed
        var overdueMock = SetupRecordMock(MaintenanceStatus.Overdue);
        var act2 = async () => await _service.UpdateStatusAsync(overdueMock.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.Completed });
        await act2.Should().ThrowAsync<AppValidationException>();

        // Completed -> InProgress
        var completedMock = SetupRecordMock(MaintenanceStatus.Completed);
        var act3 = async () => await _service.UpdateStatusAsync(completedMock.Id, new UpdateMaintenanceStatusDto { Status = MaintenanceStatus.InProgress });
        await act3.Should().ThrowAsync<AppValidationException>();
    }
}
