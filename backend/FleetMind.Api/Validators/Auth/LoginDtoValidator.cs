using FleetMind.Api.DTOs.Auth;
using FluentValidation;

namespace FleetMind.Api.Validators.Auth;

/// <summary>
/// Validates login payloads — basic presence checks only.
/// </summary>
public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}
