using System;
using FluentValidation;
using FleetMind.Api.DTOs.Fuel;

namespace FleetMind.Api.Validators.Fuel
{
    public class UpdateFuelLogDtoValidator : AbstractValidator<UpdateFuelLogDto>
    {
        public UpdateFuelLogDtoValidator()
        {
            RuleFor(x => x.QuantityLiters)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.CostPerLiter)
                .GreaterThanOrEqualTo(0).WithMessage("Cost per liter must be 0 or greater.");

            RuleFor(x => x.RecordedDate)
                .NotEmpty()
                .Must(BePastOrVeryRecent).WithMessage("Recorded date cannot be in the future (beyond a 5-minute grace window).");
        }

        private bool BePastOrVeryRecent(DateTime date)
        {
            return date <= DateTime.UtcNow.AddMinutes(5);
        }
    }
}
