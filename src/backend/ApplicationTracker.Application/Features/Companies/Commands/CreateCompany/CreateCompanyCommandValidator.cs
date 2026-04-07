using FluentValidation;

namespace ApplicationTracker.Application.Features.Companies.Commands.CreateCompany;

public class CreateCompanyCommandValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Website).MaximumLength(500).When(x => x.Website is not null);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}
