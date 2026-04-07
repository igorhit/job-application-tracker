using FluentValidation;

namespace ApplicationTracker.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Website).MaximumLength(500).When(x => x.Website is not null);
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}
