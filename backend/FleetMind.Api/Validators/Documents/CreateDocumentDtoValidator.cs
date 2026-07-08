using FluentValidation;
using FleetMind.Api.DTOs.Documents;
using FleetMind.Api.Common.Constants;
using System.Linq;

namespace FleetMind.Api.Validators.Documents;

public class CreateDocumentDtoValidator : AbstractValidator<CreateDocumentDto>
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

    public CreateDocumentDtoValidator()
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

        // Cross-field rule: EntityName and EntityId must be BOTH provided or BOTH absent
        RuleFor(x => x)
            .Must(x => (x.EntityName == null) == (x.EntityId == null))
            .WithMessage("EntityName and EntityId must be either both provided or both omitted.")
            .WithName("EntityName/EntityId");

        RuleFor(x => x.EntityName)
            .Must(name => AttachmentEntityType.Ship == name ||
                          AttachmentEntityType.Crew == name ||
                          AttachmentEntityType.Incident == name ||
                          AttachmentEntityType.DocumentVersion == name ||
                          name == null)
            .When(x => !string.IsNullOrEmpty(x.EntityName))
            .WithMessage("EntityName must be a recognized entity type.");
    }
}
