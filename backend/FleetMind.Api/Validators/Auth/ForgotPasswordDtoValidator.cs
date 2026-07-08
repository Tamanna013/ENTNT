using FluentValidation;

namespace FleetMind.Api.Validators.Auth;

public class ForgotPasswordDtoValidator : AbstractValidator<DTOs.Auth.ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");
    }
}
