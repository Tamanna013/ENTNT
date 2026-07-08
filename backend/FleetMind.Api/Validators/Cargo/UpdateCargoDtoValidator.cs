using FluentValidation;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.Common.Constants;

namespace FleetMind.Api.Validators.Cargo
{
    public class UpdateCargoDtoValidator : AbstractValidator<UpdateCargoDto>
    {
        public UpdateCargoDtoValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Cargo Status is required.")
                .Must(status => status == CargoStatus.Pending ||
                                status == CargoStatus.Loaded ||
                                status == CargoStatus.InTransit ||
                                status == CargoStatus.Delivered ||
                                status == CargoStatus.Damaged ||
                                status == CargoStatus.Lost)
                .WithMessage("Invalid Cargo Status.");

            RuleFor(x => x.WeightKg)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.");

            RuleFor(x => x.DeclaredValue)
                .GreaterThanOrEqualTo(0).WithMessage("Declared Value cannot be negative.");

            RuleFor(x => x.ConsigneeName)
                .NotEmpty().WithMessage("Consignee Name is required.")
                .MaximumLength(200).WithMessage("Consignee Name cannot exceed 200 characters.");

            // NOTE: The conditional HazardNotes validation for updates is deliberately NOT done here in the validator.
            // Since the `Type` field is not part of UpdateCargoDto (treated as immutable post-creation), this validator
            // lacks context about whether the cargo is Hazardous.
            // Instead of making the validator entity-aware (which would require fetching the entity during validation, 
            // causing awkward id-passing problems), we perform this conditional check directly in the CargoService,
            // which already loads the entity to perform the update anyway.
        }
    }
}
