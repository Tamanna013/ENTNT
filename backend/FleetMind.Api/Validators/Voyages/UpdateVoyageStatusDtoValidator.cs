using FluentValidation;
using FleetMind.Api.DTOs.Voyages;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Validators.Voyages
{
    public class UpdateVoyageStatusDtoValidator : AbstractValidator<UpdateVoyageStatusDto>
    {
        public UpdateVoyageStatusDtoValidator()
        {
            // Note: This validator only confirms the target status is a RECOGNIZED value.
            // It cannot know the voyage's CURRENT status without an extra database lookup.
            // Therefore, the actual transition-graph legality check (from state A to state B)
            // is handled in the service layer where the entity is loaded.
            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(status => 
                    status == VoyageStatus.Scheduled || 
                    status == VoyageStatus.InTransit || 
                    status == VoyageStatus.Completed || 
                    status == VoyageStatus.Cancelled || 
                    status == VoyageStatus.Delayed)
                .WithMessage("Status must be a valid known VoyageStatus value.");
        }
    }
}
