using System;
using System.Linq;
using FluentValidation;
using FleetMind.Api.DTOs.Incidents;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Incidents
{
    public class CreateIncidentDtoValidator : AbstractValidator<CreateIncidentDto>
    {
        private static readonly string[] ValidSeverities = {
            IncidentSeverity.Low,
            IncidentSeverity.Medium,
            IncidentSeverity.High,
            IncidentSeverity.Critical
        };

        public CreateIncidentDtoValidator(IUnitOfWork uow)
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

            RuleFor(x => x.OccurredAt)
                .NotEmpty()
                .Must(date => date <= DateTime.UtcNow.AddMinutes(5))
                .WithMessage("OccurredAt cannot be more than 5 minutes in the future.");

            RuleFor(x => x.ShipId)
                .NotEmpty()
                .MustAsync(async (id, cancellation) =>
                {
                    var ship = await uow.Ships.GetByIdAsync(id);
                    return ship != null;
                })
                .WithMessage("The specified Ship does not exist.");

            RuleFor(x => x)
                .MustAsync(async (dto, cancellation) =>
                {
                    if (!dto.VoyageId.HasValue) return true;
                    
                    var voyage = await uow.Voyages.GetByIdAsync(dto.VoyageId.Value);
                    if (voyage == null) return false;
                    
                    return voyage.ShipId == dto.ShipId;
                })
                .WithMessage("The specified Voyage does not exist or does not belong to the specified Ship.")
                .When(x => x.VoyageId.HasValue);
        }
    }
}
