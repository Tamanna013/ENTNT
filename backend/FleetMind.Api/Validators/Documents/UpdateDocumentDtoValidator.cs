using FluentValidation;
using FleetMind.Api.DTOs.Documents;
using FleetMind.Api.Common.Constants;
using System.Linq;

namespace FleetMind.Api.Validators.Documents;

public class UpdateDocumentDtoValidator : AbstractValidator<UpdateDocumentDto>
{
    private readonly string[] _validCategories = new[]
    {
        DocumentCategory.Regulatory,
        DocumentCategory.Insurance,
        DocumentCategory.Contract,
        DocumentCategory.Certificate,
        DocumentCategory.Policy,
        DocumentCategory.Other
    };

    public UpdateDocumentDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(c => _validCategories.Contains(c))
            .WithMessage($"Category must be one of: {string.Join(", ", _validCategories)}");

        RuleFor(x => x.Description)
            .MaximumLength(1000);
    }
}
