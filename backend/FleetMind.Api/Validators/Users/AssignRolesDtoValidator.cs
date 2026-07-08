using System.Linq;
using FleetMind.Api.DTOs.Users;
using FluentValidation;
using FleetMind.Api.Data.Seed;

namespace FleetMind.Api.Validators.Users
{
    public class AssignRolesDtoValidator : AbstractValidator<AssignRolesDto>
    {
        public AssignRolesDtoValidator()
        {
            var validRoles = RoleSeedData.GetAllRoleNames().Select(r => r.ToLower()).ToList();

            RuleFor(x => x.RoleNames)
                .NotEmpty().WithMessage("At least one role must be assigned.")
                .Must(roles => roles.All(r => validRoles.Contains(r.ToLower())))
                .WithMessage("One or more provided role names are invalid.");
        }
    }
}
