using System;
using FluentValidation;
using FleetMind.Api.DTOs.Maintenance;

namespace FleetMind.Api.Validators.Maintenance;

public class UpdateMaintenanceRecordDtoValidator : AbstractValidator<UpdateMaintenanceRecordDto>
{
    public UpdateMaintenanceRecordDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be non-negative.");

        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0).When(x => x.ActualCost.HasValue).WithMessage("Actual cost must be non-negative.");
            
        // Note: No past-date restriction on ScheduledDate for updates, 
        // as rescheduling could legitimately involve a near-past date 
        // without being as strictly prevented as initial creation.
    }
}
