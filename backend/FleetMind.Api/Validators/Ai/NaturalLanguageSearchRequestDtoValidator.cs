using FluentValidation;
using FleetMind.Api.DTOs.Ai;

namespace FleetMind.Api.Validators.Ai
{
    public class NaturalLanguageSearchRequestDtoValidator : AbstractValidator<NaturalLanguageSearchRequestDto>
    {
        public NaturalLanguageSearchRequestDtoValidator()
        {
            RuleFor(x => x.Query)
                .NotEmpty()
                .MaximumLength(500);
        }
    }
}
