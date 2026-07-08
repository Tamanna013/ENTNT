using System;
using System.Linq;
using FluentValidation;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Maintenance;

public class CreateMaintenanceRecordDtoValidator : AbstractValidator<CreateMaintenanceRecordDto>
{
    private readonly string[] _knownMaintenanceTypes = new[]
    {
        MaintenanceType.Routine,
        MaintenanceType.Emergency,
        MaintenanceType.Scheduled,
        MaintenanceType.Regulatory
    };

    public CreateMaintenanceRecordDtoValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required.")
            .Must(type => _knownMaintenanceTypes.Contains(type))
            .WithMessage("Invalid maintenance type.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        RuleFor(x => x.ScheduledDate)
            .NotEmpty().WithMessage("Scheduled date is required.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Scheduled date cannot be in the past.");

        RuleFor(x => x.EstimatedCost)
            .GreaterThanOrEqualTo(0).WithMessage("Estimated cost must be non-negative.");

        RuleFor(x => x.ShipId)
            .NotEmpty().WithMessage("ShipId is required.")
            .MustAsync(async (shipId, cancellation) => 
            {
                var ship = await unitOfWork.Ships.GetByIdAsync(shipId);
                return ship != null && !ship.IsDeleted;
            })
            .WithMessage("Ship does not exist.");
    }
}
