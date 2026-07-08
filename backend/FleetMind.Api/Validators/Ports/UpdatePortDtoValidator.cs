using FluentValidation;
using FleetMind.Api.DTOs.Ports;

namespace FleetMind.Api.Validators.Ports
{
    public class UpdatePortDtoValidator : AbstractValidator<UpdatePortDto>
    {
        public UpdatePortDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MaximumLength(150);

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
