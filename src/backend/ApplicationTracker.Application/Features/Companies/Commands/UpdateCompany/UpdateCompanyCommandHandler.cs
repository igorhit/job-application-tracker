using ApplicationTracker.Domain.Errors;
using ApplicationTracker.Domain.Interfaces;
using FluentResults;
using MediatR;

namespace ApplicationTracker.Application.Features.Companies.Commands.UpdateCompany;

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, Result>
{
    private readonly ICompanyRepository _companies;

    public UpdateCompanyCommandHandler(ICompanyRepository companies)
    {
        _companies = companies;
    }

    public async Task<Result> Handle(UpdateCompanyCommand request, CancellationToken ct)
    {
        var company = await _companies.GetByIdAsync(request.Id, request.UserId, ct);
        if (company is null)
            return Result.Fail(DomainErrors.Company.NotFound);

        company.Update(request.Name, request.Website, request.Notes);

        _companies.Update(company);
        await _companies.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
