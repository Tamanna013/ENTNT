using FluentValidation;
using FleetMind.Api.DTOs.Containers;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Containers
{
    public class UpdateContainerDtoValidator : AbstractValidator<UpdateContainerDto>
    {
        private readonly IUnitOfWork _uow;

        public UpdateContainerDtoValidator(IUnitOfWork uow)
        {
            _uow = uow;

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Container Type is required.")
                .Must(type => type == ContainerType.Dry20ft ||
                              type == ContainerType.Dry40ft ||
                              type == ContainerType.Refrigerated ||
                              type == ContainerType.OpenTop ||
                              type == ContainerType.Tank)
                .WithMessage("Invalid Container Type.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Container Status is required.")
                .Must(status => status == ContainerStatus.Empty ||
                                status == ContainerStatus.Loaded ||
                                status == ContainerStatus.InTransit ||
                                status == ContainerStatus.AtPort ||
                                status == ContainerStatus.Delivered)
                .WithMessage("Invalid Container Status.");

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
