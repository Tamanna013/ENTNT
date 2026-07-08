using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FleetMind.Api.DTOs.Crew;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Crew
{
    public class AssignToShipDtoValidator : AbstractValidator<AssignToShipDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssignToShipDtoValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.ShipId)
                .NotEmpty()
                .MustAsync(ShipExistsAsync)
                .WithMessage("The specified ship does not exist or has been deleted.");
        }

        private async Task<bool> ShipExistsAsync(Guid shipId, CancellationToken cancellationToken)
        {
            var ship = await _unitOfWork.Ships.GetByIdAsync(shipId);
            return ship != null && !ship.IsDeleted;
        }
    }
}
