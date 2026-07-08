using FluentValidation;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Containers
{
    public class CreateContainerDtoValidator : AbstractValidator<CreateContainerDto>
    {
        private readonly IUnitOfWork _uow;

        public CreateContainerDtoValidator(IUnitOfWork uow)
        {
            _uow = uow;

            RuleFor(x => x.ContainerNumber)
                .NotEmpty().WithMessage("Container Number is required.")
                .MaximumLength(20).WithMessage("Container Number cannot exceed 20 characters.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Container Type is required.")
                .Must(type => type == ContainerType.Dry20ft ||
                              type == ContainerType.Dry40ft ||
                              type == ContainerType.Refrigerated ||
                              type == ContainerType.OpenTop ||
                              type == ContainerType.Tank)
                .WithMessage("Invalid Container Type.");

            RuleFor(x => x.CurrentVoyageId)
                .MustAsync(async (id, cancellation) =>
                {
                    if (!id.HasValue) return true;
                    var voyage = await _uow.Voyages.GetByIdAsync(id.Value);
                    return voyage != null;
                })
                .When(x => x.CurrentVoyageId.HasValue)
                .WithMessage("The specified Voyage does not exist.");
        }
    }
}
