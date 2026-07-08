using System.Linq;
using FluentValidation;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Validators.Incidents
{
    public class UpdateIncidentDtoValidator : AbstractValidator<UpdateIncidentDto>
    {
        private static readonly string[] ValidSeverities = {
            IncidentSeverity.Low,
            IncidentSeverity.Medium,
            IncidentSeverity.High,
            IncidentSeverity.Critical
        };

        public UpdateIncidentDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.Description)
                .NotEmpty()
                .MaximumLength(2000);

            RuleFor(x => x.Severity)
                .NotEmpty()
                .Must(s => ValidSeverities.Contains(s))
                .WithMessage($"Severity must be one of: {string.Join(", ", ValidSeverities)}");

            RuleFor(x => x.ResolutionNotes)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.ResolutionNotes));
        }
    }
}
