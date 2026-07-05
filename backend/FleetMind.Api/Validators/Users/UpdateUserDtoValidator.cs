using FleetMind.Api.DTOs.Users;
using FluentValidation;

namespace FleetMind.Api.Validators.Users;

/// <summary>
/// Validates UpdateUserDto payloads for user profile updates.
/// </summary>
public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[\d\s\-\(\)]{7,20}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.RoleNames)
            .NotEmpty().WithMessage("At least one role must be assigned.");
    }
}
