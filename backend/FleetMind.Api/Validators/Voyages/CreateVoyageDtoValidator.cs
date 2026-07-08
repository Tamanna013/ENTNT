using FluentValidation;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Voyages
{
    public class CreateVoyageDtoValidator : AbstractValidator<CreateVoyageDto>
    {
        public CreateVoyageDtoValidator(IShipRepository shipRepository, IPortRepository portRepository)
        {
            RuleFor(x => x.ShipId)
                .NotEmpty()
                .MustAsync(async (id, cancellationToken) => 
                {
                    var ship = await shipRepository.GetByIdAsync(id);
                    return ship != null && !ship.IsDeleted;
                }).WithMessage("The specified ShipId does not exist or is deleted.");

            RuleFor(x => x.VoyageNumber)
                .NotEmpty()
                .MaximumLength(30);

            RuleFor(x => x.OriginPortId)
                .NotEmpty()
                .MustAsync(async (id, cancellation) => 
                {
                    var port = await portRepository.GetByIdAsync(id);
                    return port != null && !port.IsDeleted;
                }).WithMessage("The specified OriginPort does not exist.");

            RuleFor(x => x.DestinationPortId)
                .NotEmpty()
                .MustAsync(async (id, cancellation) => 
                {
                    var port = await portRepository.GetByIdAsync(id);
                    return port != null && !port.IsDeleted;
                }).WithMessage("The specified DestinationPort does not exist.");

            RuleFor(x => x)
                .Must(x => x.OriginPortId != x.DestinationPortId)
                .WithMessage("OriginPort and DestinationPort cannot be the same.");

            RuleFor(x => x.EstimatedArrivalDate)
                .GreaterThan(x => x.DepartureDate)
                .WithMessage("EstimatedArrivalDate must be after DepartureDate.");
        }
    }
}
