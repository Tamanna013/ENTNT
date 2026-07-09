using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Tests.TestHelpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace FleetMind.Api.Tests.Services;

public class VoyageServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IVoyageRepository> _voyageRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<INotificationRecipientResolver> _recipientMock;
    private readonly Mock<IAiProvider> _aiMock;
    private readonly VoyageService _service;

    public VoyageServiceTests()
    {
        _uowMock = MockUnitOfWorkFactory.CreateDefault();
        _voyageRepoMock = new Mock<IVoyageRepository>();
        _uowMock.Setup(u => u.Voyages).Returns(_voyageRepoMock.Object);

        // Real mapper is easier for testing returns than mocking every map call
        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Voyage, VoyageDto>();
            cfg.CreateMap<Ship, FleetMind.Api.DTOs.Ships.ShipDto>();
        });
        _mapper = config.CreateMapper();

        _notificationMock = new Mock<INotificationService>();
        _recipientMock = new Mock<INotificationRecipientResolver>();
        _aiMock = new Mock<IAiProvider>();

        _service = new VoyageService(
            _uowMock.Object,
            _mapper,
            _notificationMock.Object,
            _recipientMock.Object,
            _aiMock.Object
        );
    }

    private (Guid Id, Voyage Voyage) SetupVoyageMock(string initialStatus)
    {
        var voyageId = Guid.NewGuid();
        var voyage = new Voyage { Id = voyageId, Status = initialStatus };
        _voyageRepoMock.Setup(r => r.GetByIdWithShipAsync(voyageId)).ReturnsAsync(voyage);
        return (voyageId, voyage);
    }

    // --- Legal Transitions ---

    [Fact]
    public async Task UpdateStatusAsync_WhenScheduledToInTransit_Succeeds()
    {
        // Arrange
        var mock = SetupVoyageMock(VoyageStatus.Scheduled);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.InTransit };

        // Act
        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        // Assert
        result.Status.Should().Be(VoyageStatus.InTransit);
        mock.Voyage.Status.Should().Be(VoyageStatus.InTransit);
        _voyageRepoMock.Verify(r => r.Update(mock.Voyage), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenScheduledToCancelled_Succeeds()
    {
        var mock = SetupVoyageMock(VoyageStatus.Scheduled);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Cancelled };

        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        result.Status.Should().Be(VoyageStatus.Cancelled);
        mock.Voyage.Status.Should().Be(VoyageStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenScheduledToDelayed_Succeeds()
    {
        var mock = SetupVoyageMock(VoyageStatus.Scheduled);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Delayed };
        _recipientMock.Setup(r => r.GetUserIdsByRolesAsync(It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new List<Guid>());

        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        result.Status.Should().Be(VoyageStatus.Delayed);
        mock.Voyage.Status.Should().Be(VoyageStatus.Delayed);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenDelayedToInTransit_Succeeds()
    {
        var mock = SetupVoyageMock(VoyageStatus.Delayed);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.InTransit };

        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        result.Status.Should().Be(VoyageStatus.InTransit);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenDelayedToCancelled_Succeeds()
    {
        var mock = SetupVoyageMock(VoyageStatus.Delayed);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Cancelled };

        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        result.Status.Should().Be(VoyageStatus.Cancelled);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenInTransitToCompleted_Succeeds()
    {
        var mock = SetupVoyageMock(VoyageStatus.InTransit);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Completed };

        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        result.Status.Should().Be(VoyageStatus.Completed);
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenInTransitToDelayed_Succeeds()
    {
        var mock = SetupVoyageMock(VoyageStatus.InTransit);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Delayed };
        _recipientMock.Setup(r => r.GetUserIdsByRolesAsync(It.IsAny<string>(), It.IsAny<string[]>())).ReturnsAsync(new List<Guid>());

        var result = await _service.UpdateStatusAsync(mock.Id, dto);

        result.Status.Should().Be(VoyageStatus.Delayed);
    }

    // --- Illegal Transitions ---

    [Fact]
    public async Task UpdateStatusAsync_WhenScheduledToCompleted_ThrowsAppValidationException()
    {
        var mock = SetupVoyageMock(VoyageStatus.Scheduled);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Completed };

        var act = async () => await _service.UpdateStatusAsync(mock.Id, dto);

        await act.Should().ThrowAsync<AppValidationException>()
            .WithMessage("*Legal next states are*");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenCompletedToScheduled_ThrowsAppValidationException()
    {
        var mock = SetupVoyageMock(VoyageStatus.Completed);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Scheduled };

        var act = async () => await _service.UpdateStatusAsync(mock.Id, dto);

        await act.Should().ThrowAsync<AppValidationException>()
            .WithMessage("*Legal next states are*");
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenCancelledToScheduled_ThrowsAppValidationException()
    {
        var mock = SetupVoyageMock(VoyageStatus.Cancelled);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Scheduled };

        var act = async () => await _service.UpdateStatusAsync(mock.Id, dto);

        await act.Should().ThrowAsync<AppValidationException>()
            .WithMessage("*Legal next states are*");
    }

    // --- Specific Features ---

    [Fact]
    public async Task UpdateStatusAsync_WhenTransitioningToCompleted_AutoPopulatesActualArrivalDate()
    {
        // Arrange
        var mock = SetupVoyageMock(VoyageStatus.InTransit);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Completed, ActualArrivalDate = null };

        // Act
        await _service.UpdateStatusAsync(mock.Id, dto);

        // Assert
        mock.Voyage.ActualArrivalDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task UpdateStatusAsync_WhenTransitioningToDelayed_NotifiesFleetManagers()
    {
        // Arrange
        var mock = SetupVoyageMock(VoyageStatus.Scheduled);
        var dto = new UpdateVoyageStatusDto { Status = VoyageStatus.Delayed };
        
        var recipientIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
        _recipientMock.Setup(r => r.GetUserIdsByRolesAsync(NotificationType.VoyageDelayed, AppRoles.Admin, AppRoles.FleetManager))
                      .ReturnsAsync(recipientIds);

        // Act
        await _service.UpdateStatusAsync(mock.Id, dto);

        // Assert
        _recipientMock.Verify(r => r.GetUserIdsByRolesAsync(NotificationType.VoyageDelayed, AppRoles.Admin, AppRoles.FleetManager), Times.Once);
        
        foreach (var userId in recipientIds)
        {
            _notificationMock.Verify(n => n.CreateAsync(
                userId,
                NotificationType.VoyageDelayed,
                "Voyage Delayed",
                It.IsAny<string>(),
                "Voyage",
                mock.Id
            ), Times.Once);
        }
    }
}
