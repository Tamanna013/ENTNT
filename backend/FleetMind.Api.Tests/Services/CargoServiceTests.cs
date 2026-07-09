using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Common.Exceptions;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.Models;
using FleetMind.Api.Repositories;
using FleetMind.Api.Services;
using FleetMind.Api.Services.Ai;
using FleetMind.Api.Tests.TestHelpers;
using FleetMind.Api.Validators.Cargo;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

namespace FleetMind.Api.Tests.Services;

public class CargoServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ICargoRepository> _cargoRepoMock;
    private readonly Mock<IVoyageRepository> _voyageRepoMock;
    private readonly IMapper _mapper;
    private readonly Mock<IAiProvider> _aiProviderMock;
    private readonly CargoService _service;
    private readonly CreateCargoDtoValidator _validator;

    public CargoServiceTests()
    {
        _uowMock = MockUnitOfWorkFactory.CreateDefault();
        _cargoRepoMock = new Mock<ICargoRepository>();
        _voyageRepoMock = new Mock<IVoyageRepository>();
        
        _uowMock.Setup(u => u.Cargo).Returns(_cargoRepoMock.Object);
        _uowMock.Setup(u => u.Voyages).Returns(_voyageRepoMock.Object);

        var config = new MapperConfiguration(cfg => {
            cfg.CreateMap<Cargo, CargoDto>();
            cfg.CreateMap<CreateCargoDto, Cargo>();
            cfg.CreateMap<Voyage, FleetMind.Api.DTOs.Voyages.VoyageDto>();
        });
        _mapper = config.CreateMapper();

        _aiProviderMock = new Mock<IAiProvider>();

        _service = new CargoService(
            _uowMock.Object,
            _mapper,
            _aiProviderMock.Object
        );

        _validator = new CreateCargoDtoValidator(_uowMock.Object);
    }

    [Fact]
    public async Task CreateCargoAsync_SucceedsForHazardousCargoWithHazardNotes()
    {
        // Arrange
        var dto = new CreateCargoDto
        {
            VoyageId = Guid.NewGuid(),
            Type = CargoType.Hazardous,
            HazardNotes = "Flammable",
            Description = "Test Cargo",
            WeightKg = 100,
            ConsigneeName = "Test Consignee"
        };
        
        _cargoRepoMock.Setup(r => r.GetTotalWeightForVoyageAsync(dto.VoyageId)).ReturnsAsync(100);
        _voyageRepoMock.Setup(r => r.GetByIdAsync(dto.VoyageId)).ReturnsAsync(new Voyage { Id = dto.VoyageId });

        // Act
        var result = await _service.CreateCargoAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Type.Should().Be(CargoType.Hazardous);
        result.HazardNotes.Should().Be("Flammable");
        result.Warnings.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateCargoDtoValidator_RejectsHazardousCargoWithEmptyHazardNotes()
    {
        // Arrange
        var voyageId = Guid.NewGuid();
        _voyageRepoMock.Setup(v => v.GetByIdAsync(voyageId)).ReturnsAsync(new Voyage { Id = voyageId });

        var dto = new CreateCargoDto
        {
            VoyageId = voyageId,
            Type = CargoType.Hazardous,
            HazardNotes = "", // Empty
            Description = "Test Cargo",
            WeightKg = 100,
            ConsigneeName = "Test Consignee"
        };

        // Act
        var result = await _validator.TestValidateAsync(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.HazardNotes)
              .WithErrorMessage("Hazard notes are required for hazardous cargo.");
    }

    [Fact]
    public async Task UpdateCargoAsync_ServiceLayerCheck_RejectsClearingHazardNotesOnHazardousCargo()
    {
        // Arrange
        var cargoId = Guid.NewGuid();
        var cargo = new Cargo { Id = cargoId, Type = CargoType.Hazardous, HazardNotes = "Original Notes", VoyageId = Guid.NewGuid() };
        _cargoRepoMock.Setup(r => r.GetByIdAsync(cargoId)).ReturnsAsync(cargo);

        var updateDto = new UpdateCargoDto
        {
            Description = "Updated",
            HazardNotes = "", // Attempting to clear
            WeightKg = 100,
            ConsigneeName = "Consignee",
            Status = CargoStatus.Pending
        };

        // Act
        var act = async () => await _service.UpdateCargoAsync(cargoId, updateDto);

        // Assert
        await act.Should().ThrowAsync<AppValidationException>()
            .WithMessage("Hazard notes are required for hazardous cargo.");
    }

    [Fact]
    public async Task CreateCargoAsync_ExceedingWeightThreshold_PopulatesWarningWithoutThrowing()
    {
        // Arrange
        var dto = new CreateCargoDto
        {
            VoyageId = Guid.NewGuid(),
            Type = CargoType.GeneralGoods,
            Description = "Heavy Cargo",
            WeightKg = 1000,
            ConsigneeName = "Test Consignee"
        };

        _voyageRepoMock.Setup(r => r.GetByIdAsync(dto.VoyageId)).ReturnsAsync(new Voyage { Id = dto.VoyageId });
        
        // Mock to return a weight above the 50000kg threshold
        _cargoRepoMock.Setup(r => r.GetTotalWeightForVoyageAsync(dto.VoyageId)).ReturnsAsync(50001);

        // Act
        var result = await _service.CreateCargoAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Warnings.Should().NotBeEmpty();
        result.Warnings.Single().Should().Contain("50000 kg");
    }
}
