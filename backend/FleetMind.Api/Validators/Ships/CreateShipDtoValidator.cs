using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FleetMind.Api.DTOs.Ships;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Ships
{
    public class CreateShipDtoValidator : AbstractValidator<CreateShipDto>
    {
        private readonly IUnitOfWork _uow;

        public CreateShipDtoValidator(IUnitOfWork uow)
        {
            _uow = uow;

            var validStatuses = typeof(ShipStatus)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetRawConstantValue()?.ToString())
                .Where(v => v != null)
                .ToList();

            var validTypes = typeof(ShipType)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetRawConstantValue()?.ToString())
                .Where(v => v != null)
                .ToList();

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

            RuleFor(x => x.IMO)
                .NotEmpty().WithMessage("IMO is required.")
                .Length(7).WithMessage("IMO must be exactly 7 characters.")
                .Matches(@"^\d{7}$").WithMessage("IMO must be exactly 7 digits.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required.")
                .Must(type => validTypes.Contains(type))
                .WithMessage($"Type must be one of the following: {string.Join(", ", validTypes)}.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => validStatuses.Contains(status))
                .WithMessage($"Status must be one of the following: {string.Join(", ", validStatuses)}.");

            RuleFor(x => x.YearBuilt)
                .InclusiveBetween(1950, DateTime.UtcNow.Year).WithMessage($"YearBuilt must be between 1950 and {DateTime.UtcNow.Year}.");

            RuleFor(x => x.GrossTonnage)
                .GreaterThan(0).WithMessage("GrossTonnage must be greater than 0.");

            RuleFor(x => x.Flag)
                .NotEmpty().WithMessage("Flag is required.")
                .MaximumLength(100).WithMessage("Flag must not exceed 100 characters.");

            RuleFor(x => x.FleetId)
                .NotEmpty().WithMessage("FleetId is required.")
                .MustAsync(FleetExistsAsync).WithMessage("The specified fleet does not exist.");
        }

        private async Task<bool> FleetExistsAsync(Guid fleetId, CancellationToken cancellationToken)
        {
            var fleet = await _uow.Fleets.GetByIdAsync(fleetId);
            return fleet != null;
        }
    }
}
