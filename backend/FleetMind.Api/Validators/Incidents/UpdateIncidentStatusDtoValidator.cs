using System.Linq;
using FluentValidation;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Validators.Incidents
{
    public class UpdateIncidentStatusDtoValidator : AbstractValidator<UpdateIncidentStatusDto>
    {
        private static readonly string[] ValidStatuses = {
            IncidentStatus.Reported,
            IncidentStatus.UnderInvestigation,
            IncidentStatus.Resolved,
            IncidentStatus.Closed
        };

        public UpdateIncidentStatusDtoValidator()
        {
            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(s => ValidStatuses.Contains(s))
                .WithMessage($"Status must be one of: {string.Join(", ", ValidStatuses)}");

            RuleFor(x => x.ResolutionNotes)
                .MaximumLength(2000)
                .When(x => !string.IsNullOrEmpty(x.ResolutionNotes));
        }
    }
}
