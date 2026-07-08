using System.Linq;
using FluentValidation;
using FleetMind.Api.DTOs.Maintenance;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Validators.Maintenance;

public class UpdateMaintenanceStatusDtoValidator : AbstractValidator<UpdateMaintenanceStatusDto>
{
    private readonly string[] _knownMaintenanceStatusValues = new[]
    {
        MaintenanceStatus.Scheduled,
        MaintenanceStatus.InProgress,
        MaintenanceStatus.Completed,
        MaintenanceStatus.Overdue,
        MaintenanceStatus.Cancelled
    };

    public UpdateMaintenanceStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(status => status != MaintenanceStatus.Overdue && _knownMaintenanceStatusValues.Contains(status))
            .WithMessage("'Overdue' cannot be set manually - it is applied automatically by the system.");

        RuleFor(x => x.ActualCost)
            .GreaterThanOrEqualTo(0).When(x => x.ActualCost.HasValue).WithMessage("Actual cost must be non-negative.");
    }
}
