using System;
using FluentValidation;
using FleetMind.Api.DTOs.Fuel;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;
using System.Threading.Tasks;
using System.Threading;

namespace FleetMind.Api.Validators.Fuel
{
    public class CreateFuelLogDtoValidator : AbstractValidator<CreateFuelLogDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateFuelLogDtoValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.ShipId)
                .NotEmpty()
                .MustAsync(ShipExists).WithMessage("Ship does not exist.");

            RuleFor(x => x.FuelType)
                .NotEmpty()
                .Must(BeAValidFuelType).WithMessage("Invalid fuel type.");

            RuleFor(x => x.QuantityLiters)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.CostPerLiter)
                .GreaterThanOrEqualTo(0).WithMessage("Cost per liter must be 0 or greater.");

            RuleFor(x => x.RecordedDate)
                .NotEmpty()
                .Must(BePastOrVeryRecent).WithMessage("Recorded date cannot be in the future (beyond a 5-minute grace window).");

            RuleFor(x => x)
                .MustAsync(VoyageBelongsToShipIfProvided)
                .WithMessage("The specified voyage does not belong to the specified ship.")
                .When(x => x.VoyageId.HasValue);
        }

        private async Task<bool> ShipExists(Guid shipId, CancellationToken cancellationToken)
        {
            var ship = await _unitOfWork.Ships.GetByIdAsync(shipId);
            return ship != null;
        }

        private bool BeAValidFuelType(string fuelType)
        {
            return fuelType == FuelType.HeavyFuelOil ||
                   fuelType == FuelType.MarineDieselOil ||
                   fuelType == FuelType.LNG ||
                   fuelType == FuelType.LowSulfurFuelOil;
        }

        private bool BePastOrVeryRecent(DateTime date)
        {
            return date <= DateTime.UtcNow.AddMinutes(5);
        }

        private async Task<bool> VoyageBelongsToShipIfProvided(CreateFuelLogDto dto, CancellationToken cancellationToken)
        {
            if (!dto.VoyageId.HasValue)
                return true;

            var voyage = await _unitOfWork.Voyages.GetByIdAsync(dto.VoyageId.Value);
            if (voyage == null)
                return false; // Voyage doesn't exist at all - another rule could catch this, but failing here is safe

            return voyage.ShipId == dto.ShipId;
        }
    }
}
