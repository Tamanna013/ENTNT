using System.Linq;
using FluentValidation;
using FleetMind.Api.Common.Constants;
using FleetMind.Api.DTOs.Crew;

namespace FleetMind.Api.Validators.Crew
{
    public class UpdateCrewMemberDtoValidator : AbstractValidator<UpdateCrewMemberDto>
    {
        public UpdateCrewMemberDtoValidator()
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
                
            RuleFor(x => x.Status)
                .NotEmpty()
                .Must(status => new[] { 
                    CrewStatus.Active, CrewStatus.OnLeave, CrewStatus.Unassigned, CrewStatus.Terminated 
                }.Contains(status))
                .WithMessage("Invalid crew status.");
                
            RuleFor(x => x.Nationality).NotEmpty().MaximumLength(100);
            
            RuleFor(x => x.ContactEmail)
                .EmailAddress()
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));
        }
    }
}
