using System;
using System.Linq;
using FluentValidation;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Crew;

namespace FleetMind.Api.Validators.Crew
{
    public class CreateCrewMemberDtoValidator : AbstractValidator<CreateCrewMemberDto>
    {
        public CreateCrewMemberDtoValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
            RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
            
            RuleFor(x => x.Rank)
                .NotEmpty()
                .Must(rank => new[] { 
                    CrewRank.Captain, CrewRank.ChiefOfficer, CrewRank.ChiefEngineer, 
                    CrewRank.SecondOfficer, CrewRank.Deckhand, CrewRank.Cook, CrewRank.Cadet 
                }.Contains(rank))
                .WithMessage("Invalid crew rank.");
                
            RuleFor(x => x.Nationality).NotEmpty().MaximumLength(100);
            
            RuleFor(x => x.DateOfBirth)
                .NotEmpty()
                .Must((dto, dob) => 
                {
                    // Compute age from DateOfBirth to HireDate, not to today.
                    // This is a sanity bound rather than a strict legal requirement.
                    int age = dto.HireDate.Year - dob.Year;
                    if (dob > dto.HireDate.AddYears(-age)) age--;
                    return age >= 16 && age <= 80;
                })
                .WithMessage("Age at hire date must be between 16 and 80.");
                
            RuleFor(x => x.LicenseNumber).NotEmpty().MaximumLength(50);
            
            RuleFor(x => x.HireDate)
                .NotEmpty()
                .Must(hireDate => hireDate <= DateOnly.FromDateTime(DateTime.UtcNow))
                .WithMessage("Hire date cannot be in the future.");
                
            RuleFor(x => x.ContactEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));
        }
    }
}
