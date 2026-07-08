using System;
using FluentValidation;
using FleetMind.Api.DTOs.Containers;

namespace FleetMind.Api.Validators.Containers
{
    public class RecordTrackingEventDtoValidator : AbstractValidator<RecordTrackingEventDto>
    {
        public RecordTrackingEventDtoValidator()
        {
            RuleFor(x => x.EventType)
                .NotEmpty().WithMessage("Event Type is required.")
                .MaximumLength(50).WithMessage("Event Type cannot exceed 50 characters.");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required.")
                .MaximumLength(200).WithMessage("Location cannot exceed 200 characters.");

            RuleFor(x => x.Timestamp)
                .NotEmpty().WithMessage("Timestamp is required.")
                .Must(timestamp => timestamp <= DateTime.UtcNow.AddMinutes(5))
                .WithMessage("Timestamp cannot be more than 5 minutes in the future.");
        }
    }
}
