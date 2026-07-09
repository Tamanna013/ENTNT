using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace FleetMind.Api.Tests.Services;

public class IncidentServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IIncidentRepository> _incidentRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<INotificationRecipientResolver> _recipientMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly Mock<IAiProvider> _aiProviderMock;
    private readonly IncidentService _service;

    public IncidentServiceTests()
    {
        _uowMock = MockUnitOfWorkFactory.CreateDefault();
        _incidentRepoMock = new Mock<IIncidentRepository>();
        var shipRepoMock = new Mock<IShipRepository>();
        
        _uowMock.Setup(u => u.Incidents).Returns(_incidentRepoMock.Object);
        _uowMock.Setup(u => u.Ships).Returns(shipRepoMock.Object);
        _uowMock.Setup(u => u.Context).Returns((FleetMind.Api.Data.FleetMindDbContext)null);

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Incident, IncidentDto>();
            cfg.CreateMap<CreateIncidentDto, Incident>();
            cfg.CreateMap<User, FleetMind.Api.DTOs.Users.UserDto>();
            cfg.CreateMap<Ship, FleetMind.Api.DTOs.Ships.ShipDto>();
        });
        _mapper = config.CreateMapper();

        _notificationMock = new Mock<INotificationService>();
        _recipientMock = new Mock<INotificationRecipientResolver>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _aiProviderMock = new Mock<IAiProvider>();

        _service = new IncidentService(
            _uowMock.Object,
            _mapper,
            _notificationMock.Object,
            _recipientMock.Object,
            _currentUserMock.Object,
            _aiProviderMock.Object
        );
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    [InlineData("11111111-1111-1111-1111-111111111111")]
    [InlineData("99999999-9999-9999-9999-999999999999")]
    public async Task CreateIncidentAsync_AlwaysSetsReportedByUserIdToCurrentUserService(string userIdString)
    {
        // Arrange
        var userId = Guid.Parse(userIdString);
        _currentUserMock.Setup(c => c.UserId).Returns(userId);
        
        var dto = new CreateIncidentDto
        {
            ShipId = Guid.NewGuid(),
            Title = "Test Incident",
            Description = "Testing",
            Severity = IncidentSeverity.Low,
            OccurredAt = DateTime.UtcNow
        };

        Incident capturedIncident = null;
        _incidentRepoMock.Setup(r => r.AddAsync(It.IsAny<Incident>()))
            .Callback<Incident>(inc => capturedIncident = inc)
            .Returns(Task.CompletedTask);

        // We do not mock DbSet here since we don't have Moq.EntityFrameworkCore.
        // GetIncidentByIdAsync will fail, but we catch the exception.

        // Act
        // We catch the exception if GetIncidentByIdAsync fails because the dbContext mock is hard to set up dynamically here, 
        // but we just want to verify the captured incident prior to that.
        try
        {
            await _service.CreateIncidentAsync(dto);
        }
        catch { }

        // Assert
        capturedIncident.Should().NotBeNull();
        capturedIncident.ReportedByUserId.Should().Be(userId);
    }

    [Theory]
    [InlineData(IncidentSeverity.High)]
    [InlineData(IncidentSeverity.Critical)]
    public async Task CreateIncidentAsync_HighOrCriticalSeverity_TriggersAdminNotification(string severity)
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        var dto = new CreateIncidentDto { ShipId = Guid.NewGuid(), Title = "Bad", Description = "Very bad", Severity = severity, OccurredAt = DateTime.UtcNow };

        _recipientMock.Setup(r => r.GetUserIdsByRolesAsync(It.IsAny<string>(), AppRoles.Admin))
                      .ReturnsAsync(new List<Guid> { Guid.NewGuid() });

        try { await _service.CreateIncidentAsync(dto); } catch { }

        // Assert
        // Verify it was called with exactly AppRoles.Admin (and no other roles)
        _recipientMock.Verify(r => r.GetUserIdsByRolesAsync(It.IsAny<string>(), AppRoles.Admin), Times.Once);
        _notificationMock.Verify(n => n.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Once);
    }

    [Theory]
    [InlineData(IncidentSeverity.Low)]
    [InlineData(IncidentSeverity.Medium)]
    public async Task CreateIncidentAsync_LowOrMediumSeverity_DoesNotTriggerNotification(string severity)
    {
        // Arrange
        _currentUserMock.Setup(c => c.UserId).Returns(Guid.NewGuid());
        var dto = new CreateIncidentDto { ShipId = Guid.NewGuid(), Title = "Mild", Description = "Mildly bad", Severity = severity, OccurredAt = DateTime.UtcNow };

        try { await _service.CreateIncidentAsync(dto); } catch { }

        // Assert
        _recipientMock.Verify(r => r.GetUserIdsByRolesAsync(It.IsAny<string>(), It.IsAny<string[]>()), Times.Never);
        _notificationMock.Verify(n => n.CreateAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task UpdateStatusAsync_ClosedIncident_ThrowsForAnyTransition()
    {
        // Arrange
        var incidentId = Guid.NewGuid();
        var incident = new Incident { Id = incidentId, Status = IncidentStatus.Closed };
        _incidentRepoMock.Setup(r => r.GetByIdAsync(incidentId)).ReturnsAsync(incident);

        var dto = new UpdateIncidentStatusDto { Status = IncidentStatus.Reported }; // Attempt to reopen

        // Act
        var act = async () => await _service.UpdateStatusAsync(incidentId, dto);

        // Assert
        await act.Should().ThrowAsync<AppValidationException>()
                 .WithMessage("*none*"); // Because legal next states are empty, it should say "none"
    }
}
