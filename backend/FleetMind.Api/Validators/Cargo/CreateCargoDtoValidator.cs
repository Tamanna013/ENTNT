using FluentValidation;
using FleetMind.Api.DTOs.Cargo;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.Repositories;

namespace FleetMind.Api.Validators.Cargo
{
    public class CreateCargoDtoValidator : AbstractValidator<CreateCargoDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCargoDtoValidator(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Cargo Type is required.")
                .Must(type => type == CargoType.GeneralGoods ||
                              type == CargoType.Hazardous ||
                              type == CargoType.Perishable ||
                              type == CargoType.Bulk ||
                              type == CargoType.Liquid ||
                              type == CargoType.Vehicles)
                .WithMessage("Invalid Cargo Type.");

            RuleFor(x => x.HazardNotes)
                .NotEmpty()
                .When(x => x.Type == CargoType.Hazardous)
                .WithMessage("Hazard notes are required for hazardous cargo.");

            RuleFor(x => x.WeightKg)
                .GreaterThan(0).WithMessage("Weight must be greater than 0.");

            RuleFor(x => x.DeclaredValue)
                .GreaterThanOrEqualTo(0).WithMessage("Declared Value cannot be negative.");

            RuleFor(x => x.ConsigneeName)
                .NotEmpty().WithMessage("Consignee Name is required.")
                .MaximumLength(200).WithMessage("Consignee Name cannot exceed 200 characters.");

            RuleFor(x => x.VoyageId)
                .NotEmpty().WithMessage("VoyageId is required.")
                .MustAsync(async (id, cancellation) => 
                {
                    var voyage = await _unitOfWork.Voyages.GetByIdAsync(id);
                    return voyage != null;
                }).WithMessage("The specified Voyage does not exist.");
        }
    }
}
