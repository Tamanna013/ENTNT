using FluentValidation;
using FleetMind.Api.DTOs.Fleets;
using FleetMind.Api.Common.Constants;
using System.Linq;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Fleets
{
    public class UpdateFleetDtoValidator : AbstractValidator<UpdateFleetDto>
    {
        public UpdateFleetDtoValidator(IUnitOfWork uow, IPortRepository portRepository)
        {
            var validStatuses = typeof(FleetStatus)
                .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly)
                .Select(fi => fi.GetRawConstantValue()?.ToString())
                .Where(v => v != null)
                .ToList();

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required.")
                .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");

            RuleFor(x => x.HomePortId)
                .NotEmpty()
                .MustAsync(async (id, cancellation) => 
                {
                    var port = await portRepository.GetByIdAsync(id);
                    return port != null && !port.IsDeleted;
                }).WithMessage("The specified HomePort does not exist.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.")
                .Must(status => validStatuses.Contains(status))
                .WithMessage($"Status must be one of the following: {string.Join(", ", validStatuses)}.");
        }
    }
}
