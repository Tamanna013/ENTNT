using FluentValidation;
using FleetMind.Api.DTOs.Ports;

namespace FleetMind.Api.Validators.Ports
{
    public class CreatePortDtoValidator : AbstractValidator<CreatePortDto>
    {
        public CreatePortDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.UnLocode)
                .NotEmpty()
                .Length(5)
                .Matches(@"^[A-Z0-9]{5}$").WithMessage("UnLocode must be exactly 5 uppercase alphanumeric characters.");

            RuleFor(x => x.Country)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.City)
                .NotEmpty()
                .MaximumLength(150);

            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).When(x => x.Latitude.HasValue);

            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).When(x => x.Longitude.HasValue);
        }
    }
}
