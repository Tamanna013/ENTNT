using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FleetMind.Api.Tests.TestHelpers;
using FleetMind.Api.Validators.Fuel;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace FleetMind.Api.Tests.Services;

public class FuelLogServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IFuelLogRepository> _fuelLogRepoMock;
    private readonly Mock<IVoyageRepository> _voyageRepoMock;
    private readonly Mock<IShipRepository> _shipRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<IReportingService> _reportingMock;
    private readonly Mock<INotificationService> _notificationMock;
    private readonly Mock<INotificationRecipientResolver> _recipientMock;
    private readonly FuelLogService _service;
    private readonly CreateFuelLogDtoValidator _validator;

    public FuelLogServiceTests()
    {
        _uowMock = MockUnitOfWorkFactory.CreateDefault();
        _fuelLogRepoMock = new Mock<IFuelLogRepository>();
        _voyageRepoMock = new Mock<IVoyageRepository>();
        _shipRepoMock = new Mock<IShipRepository>();

        _uowMock.Setup(u => u.FuelLogs).Returns(_fuelLogRepoMock.Object);
        _uowMock.Setup(u => u.Voyages).Returns(_voyageRepoMock.Object);
        _uowMock.Setup(u => u.Ships).Returns(_shipRepoMock.Object);

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<FuelLog, FuelLogDto>();
            cfg.CreateMap<CreateFuelLogDto, FuelLog>();
            cfg.CreateMap<Ship, FleetMind.Api.DTOs.Ships.ShipDto>();
            cfg.CreateMap<Voyage, FleetMind.Api.DTOs.Voyages.VoyageDto>();
        });
        _mapper = config.CreateMapper();

        _reportingMock = new Mock<IReportingService>();
        _notificationMock = new Mock<INotificationService>();
        _recipientMock = new Mock<INotificationRecipientResolver>();

        _service = new FuelLogService(
            _uowMock.Object,
            _mapper,
            _reportingMock.Object,
            _notificationMock.Object,
            _recipientMock.Object
        );

        _validator = new CreateFuelLogDtoValidator(_uowMock.Object);
    }

    [Fact]
    public async Task CreateFuelLogAsync_SucceedsWhenVoyageBelongsToSameShip()
    {
        // Arrange
        var shipId = Guid.NewGuid();
        var voyageId = Guid.NewGuid();
        
        var dto = new CreateFuelLogDto
        {
            ShipId = shipId,
            VoyageId = voyageId,
            FuelType = FuelType.MarineDieselOil,
            QuantityLiters = 1000,
            CostPerLiter = 1.5m,
            RecordedDate = DateTime.UtcNow
        };

        _reportingMock.Setup(r => r.GetFuelEfficiencyReportAsync(90)).ReturnsAsync(new List<DTOs.Reporting.FuelEfficiencyReportRowDto>());

        _fuelLogRepoMock.Setup(r => r.AddAsync(It.IsAny<FuelLog>()))
            .Callback<FuelLog>(log => 
            {
                _fuelLogRepoMock.Setup(r => r.GetByIdAsync(log.Id)).ReturnsAsync(log);
            })
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.CreateFuelLogAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.ShipId.Should().Be(shipId);
        result.VoyageId.Should().Be(voyageId);
    }

    [Fact]
    public async Task CreateFuelLogDtoValidator_RejectsVoyageBelongingToDifferentShip()
    {
        // Arrange
        var fuelLogShipId = Guid.NewGuid();
        var differentShipId = Guid.NewGuid();
        var voyageId = Guid.NewGuid();

        _shipRepoMock.Setup(r => r.GetByIdAsync(fuelLogShipId)).ReturnsAsync(new Ship { Id = fuelLogShipId });
        // Voyage belongs to a DIFFERENT ship
        _voyageRepoMock.Setup(r => r.GetByIdAsync(voyageId)).ReturnsAsync(new Voyage { Id = voyageId, ShipId = differentShipId });

        var dto = new CreateFuelLogDto
        {
            ShipId = fuelLogShipId,
            VoyageId = voyageId,
            FuelType = FuelType.MarineDieselOil,
            QuantityLiters = 1000,
            CostPerLiter = 1.5m,
            RecordedDate = DateTime.UtcNow
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x)
              .WithErrorMessage("The specified voyage does not belong to the specified ship.");
    }
}
