using FluentValidation;

namespace ApplicationTracker.Application.Features.JobApplications.Commands.CreateApplication;

public class CreateApplicationCommandValidator : AbstractValidator<CreateApplicationCommand>
{
    public CreateApplicationCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(200);
        RuleFor(x => x.JobUrl).MaximumLength(500).When(x => x.JobUrl is not null);
        RuleFor(x => x.Location).MaximumLength(200).When(x => x.Location is not null);
        RuleFor(x => x.SalaryExpectation).GreaterThan(0).When(x => x.SalaryExpectation.HasValue);
        RuleFor(x => x.AppliedAt).NotEmpty();
        RuleFor(x => x.NextActionNote).MaximumLength(500).When(x => x.NextActionNote is not null);
    }
}
